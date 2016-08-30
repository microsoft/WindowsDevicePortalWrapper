using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceLab
{
    public class ObservableCommandQueue
    {
        private Queue<ICommand> commands;

        public ObservableCommandQueue()
        {
            this.commands = new Queue<ICommand>();
        }

        public event EventHandler QueueChanged;

        public int Count
        {
            get
            {
                return this.commands.Count;
            }
        }

        public void Clear()
        {
            this.commands.Clear();
            OnQueueChanged();
        }

        public ICommand Peek()
        {
            return this.commands.Peek();
        }

        public void Enqueue(ICommand cmd)
        {
            this.commands.Enqueue(cmd);
            OnQueueChanged();
        }

        public ICommand Dequeue()
        {
            ICommand cmd = this.commands.Dequeue();
            OnQueueChanged();
            return cmd;
        }

        private void OnQueueChanged()
        {
            this.QueueChanged?.Invoke(this, new EventArgs());
        }
    }


    public class CommandSequence : ICommand
    {
        private List<ICommand> registeredCommands;
        private ObservableCommandQueue commandQueue;
        private object sharedParameter;

        //-------------------------------------------------------------------
        // Constructor
        //-------------------------------------------------------------------
        #region Constructor
        public CommandSequence(ObservableCommandQueue commandQueue = null)
        {
            this.registeredCommands = new List<ICommand>();
            this.commandQueue = commandQueue == null ? new ObservableCommandQueue() : commandQueue;
            this.commandQueue.QueueChanged += CommandQueue_QueueChanged;
        }

        #endregion // Constructor

        //-------------------------------------------------------------------
        // Command Registration
        //-------------------------------------------------------------------
        public void RegisterCommand(ICommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentException(nameof(cmd));
            }

            if (cmd == this)
            {
                throw new ArgumentException("Cannot register a CommandSequence with itself");
            }

            lock (this.registeredCommands)
            {
                CommandSequence seq = cmd as CommandSequence;
                if (seq == null)
                {
                    AddCommand(cmd);
                }
                else
                {
                    foreach (ICommand subcmd in seq.registeredCommands)
                    {
                        AddCommand(subcmd);
                    }
                }
            }
            OnCanExecuteChanged();
        }

        private void AddCommand(ICommand cmd)
        {
            if(this.registeredCommands.Count == 0)
            {
                cmd.CanExecuteChanged += Forward_CanExecuteChanged;
            }
            this.registeredCommands.Add(cmd);
        }

        //-------------------------------------------------------------------
        // ICommand Implementation
        //-------------------------------------------------------------------
        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Forward on the CanExecuteChanged event from the first command in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Forward_CanExecuteChanged(object sender, EventArgs e)
        {
            this.CanExecuteChanged?.Invoke(sender, e);
        }

        public bool CanExecute(object parameter)
        {
            if (this.registeredCommands.Count == 0)
            {
                return false;
            }

            if (this.commandQueue.Count > 0)
            {
                return false;
            }

            return this.registeredCommands[0].CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            lock (this.commandQueue)
            {
                if (this.registeredCommands.Count > 0 && this.commandQueue.Count == 0)
                {
                    foreach (ICommand cmd in this.registeredCommands)
                    {
                        this.commandQueue.Enqueue(cmd);
                        this.sharedParameter = parameter;
                    }
                }
                else
                {
                    return;
                }
            }

            OnCanExecuteChanged();
            ExecuteNext();
        }

        private void CurrentCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            ICommand cmd = sender as ICommand;
            if (cmd.CanExecute(this.sharedParameter))
            {
                cmd.CanExecuteChanged -= CurrentCommand_CanExecuteChanged;
                ExecuteNext();
            }
        }

        private void ExecuteNext()
        {
            lock (this.commandQueue)
            {
                while (this.commandQueue.Count > 0 && this.commandQueue.Peek().CanExecute(this.sharedParameter))
                {
                    ICommand cmd = this.commandQueue.Dequeue();
                    cmd.Execute(this.sharedParameter);
                }

                if (this.commandQueue.Count > 0)
                {
                    this.commandQueue.Peek().CanExecuteChanged += CurrentCommand_CanExecuteChanged;
                }
            }
        }

        private void CommandQueue_QueueChanged(object sender, EventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceLab
{
    public class CommandSequence : ICommand
    {
        private List<ICommand> registeredCommands;
        private Queue<ICommand> commandQueue;
        private object sharedParameter;

        //-------------------------------------------------------------------
        // Constructor
        //-------------------------------------------------------------------
        #region Constructor
        public CommandSequence(Queue<ICommand> commandQueue = null)
        {
            this.registeredCommands = new List<ICommand>();
            this.commandQueue = commandQueue == null ? new Queue<ICommand>() : commandQueue;
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
                    this.registeredCommands.Add(cmd);
                }
                else
                {
                    foreach (ICommand subcmd in seq.registeredCommands)
                    {
                        this.registeredCommands.Add(subcmd);
                    }
                }
            }
            OnCanExecuteChanged();
        }

        //-------------------------------------------------------------------
        // ICommand Implementation
        //-------------------------------------------------------------------
        public event EventHandler CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
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
            cmd.CanExecuteChanged -= CurrentCommand_CanExecuteChanged;
            ExecuteNext();
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
                else
                {
                    OnCanExecuteChanged();
                }
            }
        }
    }
}

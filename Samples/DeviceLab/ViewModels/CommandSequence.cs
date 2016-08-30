//----------------------------------------------------------------------------------------------
// <copyright file="CommandSequence.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DeviceLab
{
    /// <summary>
    /// Composes several ICommand implementations to run in sequence with each command executing
    /// as soon as the command's predicate is satisfied. Can be used to construct sophisticated
    /// commands by sequencing several smaller commands.
    /// </summary>
    public class CommandSequence : ICommand
    {
        //-------------------------------------------------------------------
        // Constructor
        //-------------------------------------------------------------------
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSequence" /> class.
        /// </summary>
        /// <param name="commandQueue">
        /// Multiple CommandSequences may share the same ObservableCommandQueue.
        /// The predicate for the CommandSequence will return fale in the case
        /// when another command sequence is using the shared queue
        /// </param>
        public CommandSequence(ObservableCommandQueue commandQueue = null)
        {
            this.registeredCommands = new List<ICommand>();
            this.commandQueue = commandQueue == null ? new ObservableCommandQueue() : commandQueue;
            this.commandQueue.QueueChanged += this.CommandQueue_QueueChanged;
        }
        #endregion // Constructor

        //-------------------------------------------------------------------
        // Private Class Members
        //-------------------------------------------------------------------
        #region Private Class Members
        /// <summary>
        /// The ICommand instances that are composed together in this CommandSequence
        /// </summary>
        private List<ICommand> registeredCommands;

        /// <summary>
        /// The CommandSequence is executed by first placing all the commands into a queue
        /// and then executing each command as it becomes ready
        /// </summary>
        private ObservableCommandQueue commandQueue;
        private object sharedParameter;
        #endregion // Private Class Members

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

    /// <summary>
    /// A simple queue of commands that provides notifications whenever the contents changed.
    /// </summary>
    public class ObservableCommandQueue
    {
        /// <summary>
        /// Underlying Queue for the commands
        /// </summary>
        private Queue<ICommand> commands;

        //-------------------------------------------------------------------
        // Constructor
        //-------------------------------------------------------------------
        #region Constructor
        /// <summary>
        /// blah blah blah StyleCop will tell me what to put here
        /// </summary>
        public ObservableCommandQueue()
        {
            this.commands = new Queue<ICommand>();
        }
        #endregion

        //-------------------------------------------------------------------
        // Queue Operations
        //-------------------------------------------------------------------
        #region Queue Operations
        /// <summary>
        /// Gets the Count of elements contained in the queue
        /// </summary>
        public int Count
        {
            get
            {
                return this.commands.Count;
            }
        }

        /// <summary>
        /// Clears all elements from the queue
        /// </summary>
        public void Clear()
        {
            this.commands.Clear();
            this.OnQueueChanged();
        }

        /// <summary>
        /// Retrieves the element at the front of the queue without modifying the contents of the queue
        /// </summary>
        /// <returns>The element at the front of the queue</returns>
        public ICommand Peek()
        {
            return this.commands.Peek();
        }

        /// <summary>
        /// Inserts a new element at the back of the queue
        /// </summary>
        /// <param name="cmd">The new element to be inserted</param>
        public void Enqueue(ICommand cmd)
        {
            this.commands.Enqueue(cmd);
            OnQueueChanged();
        }

        /// <summary>
        /// Removes the element at the front of the queue
        /// </summary>
        /// <returns>The element that was removed from the front of the queue</returns>
        public ICommand Dequeue()
        {
            ICommand cmd = this.commands.Dequeue();
            this.OnQueueChanged();
            return cmd;
        }
        #endregion // Queue Operations

        //-------------------------------------------------------------------
        // QueueChanged Event
        //-------------------------------------------------------------------
        #region QueueChanged Event
        public event EventHandler QueueChanged;

        private void OnQueueChanged()
        {
            this.QueueChanged?.Invoke(this, new EventArgs());
        }
        #endregion // QueueChanged Event
    }
}

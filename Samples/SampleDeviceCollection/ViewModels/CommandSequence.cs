//----------------------------------------------------------------------------------------------
// <copyright file="CommandSequence.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SampleDeviceCollection
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
        
        /// <summary>
        /// The parameter passed to execute and subsequently shared with each command when it executes
        /// </summary>
        private object sharedParameter;
        #endregion // Private Class Members

        //-------------------------------------------------------------------
        // Command Registration
        //-------------------------------------------------------------------
        #region Command Registration
        /// <summary>
        /// Register a command with the CommandSequence
        /// Commands are composed in the same order that they are registered
        /// </summary>
        /// <param name="cmd">Command to register with this CommandSequence</param>
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
                    this.AddCommand(cmd);
                }
                else
                {
                    // If the command is itself a CommandSequence then we crack it open
                    // and add the individual commands within it. This enables composing
                    // commands with other CommandSequences. Flattening is required in
                    // the case when multiple command sequences share the same queue
                    foreach (ICommand subcmd in seq.registeredCommands)
                    {
                        this.AddCommand(subcmd);
                    }
                }
            }

            this.OnCanExecuteChanged();
        }

        /// <summary>
        /// Internal helper to add a command to the list of commands.
        /// </summary>
        /// <param name="cmd">Command to add to this CommandSequence</param>
        private void AddCommand(ICommand cmd)
        {
            if (this.registeredCommands.Count == 0)
            {
                cmd.CanExecuteChanged += this.Forward_CanExecuteChanged;
            }

            this.registeredCommands.Add(cmd);
        }
        #endregion // Command Registration

        //-------------------------------------------------------------------
        // ICommand Implementation
        //-------------------------------------------------------------------
        #region ICommand Implementation
        #region CanExecuteChanged event
        /// <summary>
        /// Event signals when the ability to execute the CommandSequence has changed
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Invoke the CanExecuteChanged event handler
        /// </summary>
        private void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Forward on the CanExecuteChanged event from the first command in the list.
        /// See CanExecute for the conditions under which the CommandSequence can execute.
        /// </summary>
        /// <param name="sender">Originator of the event</param>
        /// <param name="e">The arguments associated with this event</param>
        private void Forward_CanExecuteChanged(object sender, EventArgs e)
        {
            this.CanExecuteChanged?.Invoke(sender, e);
        }
        #endregion // CanExecuteChanged event

        /// <summary>
        /// Predicate indicates whether the CommandSequence is ready to execute
        /// </summary>
        /// <param name="parameter">Value passed to Execute/CanExecute for the commands in the sequence</param>
        /// <returns>Indicates whether this CommandSequence is ready to execute</returns>
        public bool CanExecute(object parameter)
        {
            return
                this.registeredCommands.Count > 0                    // Must have at least one command to execute
                && this.commandQueue.Count == 0                      // The queue must not be in use already
                && this.registeredCommands[0].CanExecute(parameter); // First command must be ready to execute
        }

        /// <summary>
        /// Execute each command in the CommandSequence as they become ready
        /// </summary>
        /// <param name="parameter">The command parameter to be passed to all of the commands</param>
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

            this.OnCanExecuteChanged();
            this.ExecuteNext();
        }

        /// <summary>
        /// Event handler used determine when the command at the head of the queue is ready to execute
        /// </summary>
        /// <param name="sender">The originator of the event should be the command at the front of the queue</param>
        /// <param name="e">The arguments associated with this event</param>
        private void CurrentCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            ICommand cmd = sender as ICommand;
            if (cmd.CanExecute(this.sharedParameter))
            {
                cmd.CanExecuteChanged -= this.CurrentCommand_CanExecuteChanged;
                this.ExecuteNext();
            }
        }

        /// <summary>
        /// Execute all the commands from the front of the queue that are already ready then hook up the
        /// event to receive the signal when the next one is ready.
        /// </summary>
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
                    this.commandQueue.Peek().CanExecuteChanged += this.CurrentCommand_CanExecuteChanged;
                }
            }
        }

        /// <summary>
        /// The ObservableCommandQueue informs the CommandSequence when queue contents change so
        /// that the CommandSequence can fire the CanExecuteChanged.
        /// </summary>
        /// <param name="sender">ObservableCommandQueue that originated the event</param>
        /// <param name="e">The arguments associated with this event</param>
        private void CommandQueue_QueueChanged(object sender, EventArgs e)
        {
            this.OnCanExecuteChanged();
        }
        #endregion // ICommand Implementation
    }
}

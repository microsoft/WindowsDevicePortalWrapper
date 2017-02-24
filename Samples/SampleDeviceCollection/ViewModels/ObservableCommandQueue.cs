//----------------------------------------------------------------------------------------------
// <copyright file="ObservableCommandQueue.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SampleDeviceCollection
{
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
        /// Initializes a new instance of the <see cref="ObservableCommandQueue" /> class.
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
            this.OnQueueChanged();
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
        /// <summary>
        /// Event for when there are changes to the queue's contents
        /// </summary>
        public event EventHandler QueueChanged;

        /// <summary>
        /// Invokes the QueueChanged event
        /// </summary>
        private void OnQueueChanged()
        {
            this.QueueChanged?.Invoke(this, new EventArgs());
        }
        #endregion // QueueChanged Event
    }
}

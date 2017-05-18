//----------------------------------------------------------------------------------------------
// <copyright file="NumberEntryBox.xaml.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Interaction logic for NumberEntryBox.xaml
    /// </summary>
    public partial class NumberEntryBox : UserControl, INotifyPropertyChanged
    {
        //-------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberEntryBox" /> class.
        /// </summary>
        public NumberEntryBox()
        {
            this.InitializeComponent();
        }
        #endregion // Constructors

        //-------------------------------------------------------------------
        // Dependency Properties
        //-------------------------------------------------------------------
        #region DependencyProperties
        #region Value Dependency Property
        /// <summary>
        /// Gets or sets the instance property backing the Value Dependency Property
        /// </summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Value Dependency Property static association
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumberEntryBox), new PropertyMetadata(0, OnValueChanged, CoerceValue));

        /// <summary>
        /// Forward the dependency property changes through the INotifyPropertyChanged interface
        /// </summary>
        /// <param name="d">Dependency opbject that bears the property's value</param>
        /// <param name="e">The arguments associated with this event</param>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberEntryBox thisNEB = d as NumberEntryBox;
            if (thisNEB != null)
            {
                thisNEB.PropertyChanged?.Invoke(thisNEB, new PropertyChangedEventArgs("Value"));
            }
        }

        /// <summary>
        /// Coerce the value to keep it inside the expected range. The value should not be less than 1
        /// </summary>
        /// <param name="d">Dependency opbject that bears the property's value</param>
        /// <param name="baseValue">The value the property would have taken before being coerced</param>
        /// <returns>The coerced value</returns>
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            int val = (int)baseValue > 0 ? (int)baseValue : 1;
            return val;
        }
        #endregion // Value Dependency Property
        #endregion // Dependency Properties

        //-------------------------------------------------------------------
        // Commands
        //-------------------------------------------------------------------
        #region Commands
        #region DecrementCommand
        /// <summary>
        /// The DecrementCommand decrements the number in the Value Depencency Property
        /// </summary>
        private DelegateCommand decrementCommand;
        
        /// <summary>
        /// Gets the DecrementCommand
        /// </summary>
        public ICommand DecrementCommand
        {
            get
            {
                if (this.decrementCommand == null)
                {
                    this.decrementCommand = new DelegateCommand(this.ExecuteDecrement, this.CanExecuteDecrement);
                    this.decrementCommand.ObservesProperty(() => this.Value);
                }

                return this.decrementCommand;
            }
        }

        /// <summary>
        /// Predicate for the DecrementCommand
        /// </summary>
        /// <returns>True if the command can be executed</returns>
        private bool CanExecuteDecrement()
        {
            return this.Value > 1;
        }

        /// <summary>
        /// Performs the operation of the DecrementCommand
        /// </summary>
        private void ExecuteDecrement()
        {
            this.Value = this.Value - 1;
        }
        #endregion // DecrementCommand

        #region IncrementCommand
        /// <summary>
        /// The DecrementCommand decrements the number in the Value Depencency Property
        /// </summary>
        private DelegateCommand incrementCommand;

        /// <summary>
        /// Gets the IncrementCommand
        /// </summary>
        public ICommand IncrementCommand
        {
            get
            {
                if (this.incrementCommand == null)
                {
                    this.incrementCommand = new DelegateCommand(this.ExecuteIncrement);
                }

                return this.incrementCommand;
            }
        }

        /// <summary>
        /// Performs the operation of the Increment command
        /// </summary>
        private void ExecuteIncrement()
        {
            this.Value = this.Value + 1;
        }
        #endregion // IncrementCommand
        #endregion // Commands

        //-------------------------------------------------------------------
        // INotifyPropertyChanged implementation
        //-------------------------------------------------------------------
        #region INotifyPropertyChanged implementation
        /// <summary>
        /// PropertyChanged event for the INotifyPropertyChanged implementation
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion // INotifyPropertyChanged implementation
    }
}

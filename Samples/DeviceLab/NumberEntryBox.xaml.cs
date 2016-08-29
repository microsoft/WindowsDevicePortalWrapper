using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceLab
{
    /// <summary>
    /// Interaction logic for NumberEntryBox.xaml
    /// </summary>
    public partial class NumberEntryBox : UserControl, INotifyPropertyChanged
    {
        public NumberEntryBox()
        {
            InitializeComponent();
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumberEntryBox), new PropertyMetadata(0, OnValueChanged, CoerceValue));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberEntryBox thisNEB = d as NumberEntryBox;
            if (thisNEB != null)
            {
                thisNEB.PropertyChanged?.Invoke(thisNEB, new PropertyChangedEventArgs("Value"));
            }
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            int val = (int)baseValue > 0 ? (int)baseValue : 1;
            return val;
        }

        //-------------------------------------------------------------------
        // Commands
        //-------------------------------------------------------------------
        #region Commands
        #region DecrementCommand
        private DelegateCommand decrementCommand;
        public ICommand DecrementCommand
        {
            get
            {
                if(this.decrementCommand == null)
                {
                    this.decrementCommand = new DelegateCommand(ExecuteDecrement, CanExecuteDecrement);
                    this.decrementCommand.ObservesProperty(() => Value);
                }
                return this.decrementCommand;
            }
        }

        private bool CanExecuteDecrement()
        {
            return this.Value > 1;
        }

        private void ExecuteDecrement()
        {
            this.Value = this.Value - 1;
        }
        #endregion // DecrementCommand

        #region IncrementCommand
        private DelegateCommand incrementCommand;
        public ICommand IncrementCommand
        {
            get
            {
                if(this.incrementCommand == null)
                {
                    this.incrementCommand = new DelegateCommand(ExecuteIncrement);
                }
                return this.incrementCommand;
            }
        }

        private void ExecuteIncrement()
        {
            this.Value = this.Value + 1;
        }
        #endregion // IncrementCommand
        #endregion // Commands

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

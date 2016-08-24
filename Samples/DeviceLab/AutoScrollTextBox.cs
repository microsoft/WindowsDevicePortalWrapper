using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeviceLab
{
    public class AutoScrollTextBox : TextBox
    {
        static AutoScrollTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoScrollTextBox), new FrameworkPropertyMetadata(typeof(AutoScrollTextBox)));
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            CaretIndex = Text.Length;
            ScrollToEnd();
        }
    }
}

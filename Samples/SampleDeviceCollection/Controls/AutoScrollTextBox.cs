//----------------------------------------------------------------------------------------------
// <copyright file="AutoScrollTextBox.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace SampleDeviceCollection
{
    /// <summary>
    /// A TextBox derrivative that automatically scrolls to end whenever new text is added.
    /// This is used for presenting scrolling output spew.
    /// </summary>
    public class AutoScrollTextBox : TextBox
    {
        /// <summary>
        /// Initializes static members of the <see cref="AutoScrollTextBox" /> class.
        /// </summary>
        static AutoScrollTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoScrollTextBox), new FrameworkPropertyMetadata(typeof(AutoScrollTextBox)));
        }

        /// <summary>
        /// Override OnTextChanged to automatically scroll to the end
        /// </summary>
        /// <param name="e">The arguments associated with this event</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.CaretIndex = Text.Length;
            this.ScrollToEnd();
        }
    }
}

//----------------------------------------------------------------------------------------------
// <copyright file="DeviceCollectionView.xaml.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows.Input;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Interaction logic for DeviceCollectionView.xaml
    /// </summary>
    public partial class DeviceCollectionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceCollectionView" /> class.
        /// </summary>
        public DeviceCollectionView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Eats any mouse clicks so that they won't be handled by the ListBox control
        /// </summary>
        /// <param name="sender">Object that originated the event</param>
        /// <param name="e">Arguments for the event</param>
        private void EatMouseClicks(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}

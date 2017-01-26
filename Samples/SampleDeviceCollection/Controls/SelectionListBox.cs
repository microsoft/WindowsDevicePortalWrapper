//----------------------------------------------------------------------------------------------
// <copyright file="SelectionListBox.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SampleDeviceCollection
{
    /// <summary>
    /// Custom ListBox that exposes a SelectionList Dependency property to enable two-way binding.
    /// Internally, SelectionList is kept in sink with the SelectedItems property of the base class
    /// </summary>
    public class SelectionListBox : ListBox
    {
        //-------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------
        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="SelectionListBox" /> class.
        /// </summary>
        static SelectionListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionListBox), new FrameworkPropertyMetadata(typeof(SelectionListBox)));
        }
        #endregion // Cosntructors

        //-------------------------------------------------------------------
        // Depenency Properties
        //-------------------------------------------------------------------
        #region Dependency Properties
        #region SelectionList Dependency Property

        /// <summary>
        /// Gets or sets the SelectionList dependency property for the instance of the class
        /// </summary>
        public IList SelectionList
        {
            get { return (IList)GetValue(SelectionListProperty); }
            set { this.SetValue(SelectionListProperty, value); }
        }

        /// <summary>
        /// SelectionList Dependency Property static association
        /// </summary>
        public static readonly DependencyProperty SelectionListProperty =
            DependencyProperty.Register("SelectionList", typeof(IList), typeof(SelectionListBox), new PropertyMetadata(null, null, CoerceSelectionList));
 
        /// <summary>
        /// Coerce the value of SelectionList so that it is identical to (i.e. the same object as) the
        /// value of SelectedItems
        /// </summary>
        /// <param name="d">SelectionListBox instance</param>
        /// <param name="baseValue">New list of selected items</param>
        /// <returns>The list of selected items of the list</returns>
        private static object CoerceSelectionList(DependencyObject d, object baseValue)
        {
            SelectionListBox thisSLB = d as SelectionListBox;
            IList selectedItems = thisSLB.SelectedItems;
            IList baseList = baseValue as IList;
            if (baseList == selectedItems)
            {
                // Must have been called from OnSelectionChanged...
                // ...implies nothing to do so early out
                thisSLB.OnPropertyChanged(new DependencyPropertyChangedEventArgs(SelectionListProperty, null, selectedItems));
                return selectedItems;
            }

            // baseValue is not identical to SelectedItems, so fixup SelectedItems
            // to have the same elements as baseValue...
            // Note: This will result in multiple calls to OnSelectionChanged which
            // will subsequently call this method but will early out according to
            // the condition described above.
            thisSLB.SelectedItems.Clear();
            if (baseList != null)
            {
                foreach (object itm in baseList)
                {
                    thisSLB.SelectedItems.Add(itm);
                }
            }

            return selectedItems;
        }

        /// <summary>
        /// OnSelectionChange needs to drive changes to the SelectionList Dependency Property
        /// </summary>
        /// <param name="e">The arguments associated with this event</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.SetValue(SelectionListProperty, this.SelectedItems);
        }
        #endregion // SelectionList Dependency Property
        #endregion // Dependency Properties
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeviceLab
{
    /// <summary>
    /// Custom ListBox that exposes a SelectionList Dependency property to enable two-way binding.
    /// Internally, SelectionList is kept in sink with the SelectedItems property of the base
    /// class
    /// </summary>
    public class SelectionListBox : ListBox
    {
        static SelectionListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionListBox), new FrameworkPropertyMetadata(typeof(SelectionListBox)));
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(SelectionListProperty, this.SelectedItems);
        }

        public IList SelectionList
        {
            get { return (IList)GetValue(SelectionListProperty); }
            set { SetValue(SelectionListProperty, value); }
        }

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
            SelectionListBox thisDCV = d as SelectionListBox;
            IList selectedItems = thisDCV.SelectedItems;
            IList baseList = baseValue as IList;
            if (baseList == selectedItems)
            {
                // Must have been called from OnSelectionChanged...
                // ...implies nothing to do so early out
                return selectedItems;
            }

            // baseValue is not identical to SelectedItems, so fixup SelectedItems
            // to have the same elements as baseValue...
            // Note: This will result in multiple calls to OnSelectionChanged which
            // will subsequently call this method but will early out according to
            // the condition described above.
            thisDCV.SelectedItems.Clear();
            if (baseList != null)
            {
                foreach (object itm in baseList)
                {
                    thisDCV.SelectedItems.Add(itm);
                }
            }

            return selectedItems;
        }
    }
}

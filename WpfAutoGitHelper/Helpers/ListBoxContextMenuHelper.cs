using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfAutoGitHelper.Helpers
{
    internal static class ListBoxContextMenuHelper
    {
        public static void SelectItemUnderMouse(ListBox listBox, MouseButtonEventArgs e)
        {
            if (listBox == null)
                return;

            var position = e.GetPosition(listBox);
            var hit = listBox.InputHitTest(position) as DependencyObject;
            if (hit == null)
                return;

            var container = ItemsControl.ContainerFromElement(listBox, hit) as ListBoxItem;
            if (container == null)
                return;

            listBox.SelectedItem = container.DataContext ?? container.Content;
        }
    }
}

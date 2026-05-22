using System.Windows.Controls;
using System.Windows.Input;
using WpfAutoGitHelper.Helpers;

namespace WpfAutoGitHelper.Views
{
    public partial class AdvancedModeView
    {
        public AdvancedModeView()
        {
            InitializeComponent();
        }

        private void OnListBoxPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox)
                ListBoxContextMenuHelper.SelectItemUnderMouse(listBox, e);
        }
    }
}

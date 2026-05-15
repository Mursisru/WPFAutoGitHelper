using System.Windows;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Services;
using WpfAutoGitHelper.ViewModels;

namespace WpfAutoGitHelper
{
    public partial class NewRepositoryDialog : Window
    {
        public NewRepositoryDialog(string initialParentDirectory, string themeId)
        {
            InitializeComponent();
            DataContext = new NewRepositoryDialogViewModel(initialParentDirectory);
            ThemeManager.ApplyTo(this, themeId);
        }

        public NewRepositoryRequest Result { get; private set; }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var vm = (NewRepositoryDialogViewModel)DataContext;
            if (!vm.TryBuildRequest(out var request, out var errorKey))
            {
                MessageBox.Show(
                    Loc.Get(errorKey),
                    vm.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Result = request;
            DialogResult = true;
        }
    }
}

using System.Windows;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.Views
{
    public partial class FatalErrorWindow : Window
    {
        public FatalErrorWindow(string errorText, string title = null)
        {
            InitializeComponent();
            DataContext = new FatalErrorViewModel(errorText, title);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private sealed class FatalErrorViewModel
        {
            public FatalErrorViewModel(string errorText, string title)
            {
                ErrorText = errorText ?? "";
                TitleText = string.IsNullOrWhiteSpace(title)
                    ? Loc.Get("Dlg_StartupErrorTitle")
                    : title;
                OkText = Loc.Get("Btn_DialogOk");
            }

            public string ErrorText { get; }
            public string TitleText { get; }
            public string OkText { get; }
        }
    }
}

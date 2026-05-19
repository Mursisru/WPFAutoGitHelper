using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.Views
{
    public partial class ConflictResolutionWindow : Window, INotifyPropertyChanged
    {
        private readonly string _filePath;
        private string _originalText;
        private string _baseText = "";
        private string _oursText = "";
        private string _theirsText = "";
        private ConflictResolutionChoice _choice = ConflictResolutionChoice.Ours;

        public string FilePath => _filePath;

        public string BaseText
        {
            get => _baseText;
            private set { _baseText = value ?? ""; OnPropertyChanged(); }
        }

        public string OursText
        {
            get => _oursText;
            private set { _oursText = value ?? ""; OnPropertyChanged(); }
        }

        public string TheirsText
        {
            get => _theirsText;
            private set { _theirsText = value ?? ""; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ConflictResolutionWindow(string repoPath, string relativePath, string fullPath)
        {
            InitializeComponent();
            _filePath = fullPath;
            DataContext = this;
            LoadFile();
            _ = LoadGitStagesAsync(repoPath, relativePath);
        }

        private void LoadFile()
        {
            _originalText = File.ReadAllText(_filePath);
            var hunks = ConflictMarkerParser.Parse(_originalText);
            if (hunks.Count == 0)
            {
                OursText = _originalText;
                TheirsText = "";
                return;
            }

            var first = hunks[0];
            OursText = first.Ours;
            TheirsText = first.Theirs;
        }

        private async Task LoadGitStagesAsync(string repoPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(repoPath) || string.IsNullOrWhiteSpace(relativePath))
                return;

            var gitPath = relativePath.Replace('\\', '/');
            var baseResult = await GitRunner.RunAsync(repoPath, CancellationToken.None, "show", ":1:" + gitPath).ConfigureAwait(true);
            if (baseResult.Success && !string.IsNullOrWhiteSpace(baseResult.StandardOutput))
                await Dispatcher.InvokeAsync(() => BaseText = baseResult.StandardOutput);

            var oursResult = await GitRunner.RunAsync(repoPath, CancellationToken.None, "show", ":2:" + gitPath).ConfigureAwait(true);
            if (oursResult.Success && !string.IsNullOrWhiteSpace(oursResult.StandardOutput))
                await Dispatcher.InvokeAsync(() => OursText = oursResult.StandardOutput);

            var theirsResult = await GitRunner.RunAsync(repoPath, CancellationToken.None, "show", ":3:" + gitPath).ConfigureAwait(true);
            if (theirsResult.Success && !string.IsNullOrWhiteSpace(theirsResult.StandardOutput))
                await Dispatcher.InvokeAsync(() => TheirsText = theirsResult.StandardOutput);
        }

        private void OnAcceptOurs(object sender, RoutedEventArgs e) => _choice = ConflictResolutionChoice.Ours;
        private void OnAcceptTheirs(object sender, RoutedEventArgs e) => _choice = ConflictResolutionChoice.Theirs;
        private void OnAcceptBoth(object sender, RoutedEventArgs e) => _choice = ConflictResolutionChoice.Both;

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var hunks = ConflictMarkerParser.Parse(_originalText);
            if (hunks.Any())
            {
                var resolved = ConflictMarkerParser.ApplyResolution(_originalText, hunks, _choice);
                File.WriteAllText(_filePath, resolved);
            }

            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private const string UnknownBranch = "-";

        private readonly AppSettings _settings;
        private CancellationTokenSource _operationCts;

        private string _repoPath = "";
        private string _commitMessage = "";
        private string _userName = "";
        private string _userEmail = "";
        private string _newBranchName = "";
        private string _selectedBranch = "";
        private string _currentBranch = UnknownBranch;
        private string _logText = "";
        private bool _isBusy;
        private bool _hasValidRepo;
        private string _selectedLanguageCode = Loc.DefaultLanguage;
        private string _selectedThemeId = ThemeManager.Light;
        private bool _confirmCommit = true;
        private bool _autoRefreshOnSaveRepo = true;
        private string _releaseTag = "";
        private string _releaseTitle = "";
        private string _releaseNotes = "";
        private string _releaseTargetBranch = "";
        private bool _releaseLatest;
        private bool _releasePrerelease;
        private bool _persistSettingsEnabled;

        public MainViewModel()
        {
            _settings = SettingsStore.Load();
            _persistSettingsEnabled = false;
            Ui = new LocalizedUi();

            RepoPath = _settings.RepoPath ?? "";
            CommitMessage = _settings.LastCommitMessage ?? "";
            SelectedThemeId = string.IsNullOrWhiteSpace(_settings.Theme) ? ThemeManager.Light : _settings.Theme;
            ConfirmCommit = _settings.ConfirmCommit;
            AutoRefreshOnSaveRepo = _settings.AutoRefreshOnSaveRepo;
            ReleaseTag = _settings.LastReleaseTag ?? "";
            ReleaseTitle = _settings.LastReleaseTitle ?? "";
            ReleaseNotes = _settings.LastReleaseNotes ?? "";

            foreach (var p in _settings.RecentRepoPaths ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(p) && !RecentRepoPaths.Contains(p))
                    RecentRepoPaths.Add(p);
            }

            Loc.LanguageChanged += OnLanguageChanged;
            _selectedLanguageCode = Loc.Normalize(_settings.Language);
            Loc.ApplyLanguage(_selectedLanguageCode);
            ThemeManager.Apply(SelectedThemeId);
            RefreshThemeOptions();
            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(SelectedTheme));

            BrowseRepoCommand = new RelayCommand(BrowseRepo);
            CreateNewRepoCommand = new RelayCommand(async () => await CreateNewRepoAsync(), () => !IsBusy);
            SaveRepoCommand = new RelayCommand(SaveRepo, () => !IsBusy);
            OpenFolderCommand = new RelayCommand(OpenFolder, () => HasValidRepo && !IsBusy);
            OpenGitHubCommand = new RelayCommand(async () => await OpenGitHubAsync(), () => HasValidRepo && !IsBusy);
            RefreshStatusCommand = new RelayCommand(async () => await RefreshStatusAsync(), () => HasValidRepo && !IsBusy);
            PullCommand = new RelayCommand(async () => await PullAsync(), () => HasValidRepo && !IsBusy);
            DiffCommand = new RelayCommand(async () => await DiffAsync(), () => HasValidRepo && !IsBusy);
            AddAllCommand = new RelayCommand(async () => await AddAllAsync(), () => HasValidRepo && !IsBusy);
            CommitCommand = new RelayCommand(async () => await CommitAsync(), () => HasValidRepo && !IsBusy);
            PushCommand = new RelayCommand(async () => await PushAsync(), () => HasValidRepo && !IsBusy);
            ClearWorkflowCommand = new RelayCommand(ClearWorkflow, () => !IsBusy);
            CreateReleaseCommand = new RelayCommand(async () => await CreateReleaseAsync(), () => HasValidRepo && !IsBusy);
            OpenReleasesCommand = new RelayCommand(async () => await OpenReleasesPageAsync(), () => HasValidRepo && !IsBusy);
            LoadGitConfigCommand = new RelayCommand(async () => await LoadGitConfigAsync(), () => !IsBusy);
            ApplyGitConfigCommand = new RelayCommand(async () => await ApplyGitConfigAsync(), () => !IsBusy);
            ClearIdentityCommand = new RelayCommand(async () => await ClearGitIdentityAsync(), () => !IsBusy);
            CreateBranchCommand = new RelayCommand(async () => await CreateBranchAsync(), () => HasValidRepo && !IsBusy);
            CheckoutBranchCommand = new RelayCommand(async () => await CheckoutBranchAsync(), () => HasValidRepo && !IsBusy && !string.IsNullOrWhiteSpace(SelectedBranch));
            PushBranchCommand = new RelayCommand(async () => await PushBranchAsync(), () => HasValidRepo && !IsBusy);
            ClearLogCommand = new RelayCommand(ClearLog, () => !string.IsNullOrEmpty(LogText));

            _persistSettingsEnabled = true;
            PersistSettings();

            ValidateRepo();
            if (HasValidRepo)
                _ = SafeRefreshOnStartupAsync();
            else if (GitRunner.FindGitExecutable() == null)
                AppendLog(Loc.Get("Msg_GitNotFound"), true);
        }

        public LocalizedUi Ui { get; }

        public ObservableCollection<LanguageOption> Languages { get; } =
            new ObservableCollection<LanguageOption>(Loc.AvailableLanguages);

        public ObservableCollection<ThemeOption> Themes { get; } = new ObservableCollection<ThemeOption>();

        private async Task SafeRefreshOnStartupAsync()
        {
            try
            {
                await RefreshStatusAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format(Loc.Get("Msg_StartupRefreshFailed"), ex.Message), true);
            }
        }

        public ObservableCollection<string> RecentRepoPaths { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();

        public LanguageOption SelectedLanguage
        {
            get
            {
                foreach (var lang in Languages)
                {
                    if (string.Equals(lang.Code, _selectedLanguageCode, StringComparison.OrdinalIgnoreCase))
                        return lang;
                }

                return Languages.Count > 0 ? Languages[0] : null;
            }
            set
            {
                if (value == null)
                    return;

                var code = Loc.Normalize(value.Code);
                if (string.Equals(_selectedLanguageCode, code, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedLanguageCode = code;
                Loc.ApplyLanguage(code);
                OnPropertyChanged(nameof(SelectedLanguageCode));
                OnPropertyChanged(nameof(SelectedLanguage));
                PersistSettings();
            }
        }

        public string SelectedLanguageCode
        {
            get => _selectedLanguageCode;
            set
            {
                var code = Loc.Normalize(value);
                if (string.Equals(_selectedLanguageCode, code, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedLanguageCode = code;
                Loc.ApplyLanguage(code);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedLanguage));
                PersistSettings();
            }
        }

        public ThemeOption SelectedTheme
        {
            get
            {
                foreach (var theme in Themes)
                {
                    if (string.Equals(theme.Id, _selectedThemeId, StringComparison.OrdinalIgnoreCase))
                        return theme;
                }

                return Themes.Count > 0 ? Themes[0] : null;
            }
            set
            {
                if (value == null)
                    return;

                var themeId = value.Id == ThemeManager.Dark ? ThemeManager.Dark : ThemeManager.Light;
                if (string.Equals(_selectedThemeId, themeId, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedThemeId = themeId;
                ThemeManager.Apply(themeId);
                OnPropertyChanged(nameof(SelectedThemeId));
                OnPropertyChanged(nameof(SelectedTheme));
                PersistSettings();
            }
        }

        public string SelectedThemeId
        {
            get => _selectedThemeId;
            set
            {
                var theme = string.IsNullOrWhiteSpace(value) ? ThemeManager.Light : value.Trim().ToLowerInvariant();
                if (theme != ThemeManager.Dark)
                    theme = ThemeManager.Light;
                if (string.Equals(_selectedThemeId, theme, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedThemeId = theme;
                ThemeManager.Apply(theme);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedTheme));
                PersistSettings();
            }
        }

        public bool ConfirmCommit
        {
            get => _confirmCommit;
            set
            {
                if (_confirmCommit == value)
                    return;
                _confirmCommit = value;
                OnPropertyChanged();
                PersistSettings();
            }
        }

        public bool AutoRefreshOnSaveRepo
        {
            get => _autoRefreshOnSaveRepo;
            set
            {
                if (_autoRefreshOnSaveRepo == value)
                    return;
                _autoRefreshOnSaveRepo = value;
                OnPropertyChanged();
                PersistSettings();
            }
        }

        public string BusyText => string.Format(Loc.Get("Busy_Format"), IsBusy);

        public string RepoPath
        {
            get => _repoPath;
            set
            {
                if (_repoPath == value)
                    return;
                _repoPath = value ?? "";
                OnPropertyChanged();
                ValidateRepo();
            }
        }

        public string CommitMessage
        {
            get => _commitMessage;
            set { _commitMessage = value ?? ""; OnPropertyChanged(); }
        }

        public string UserName
        {
            get => _userName;
            set { _userName = value ?? ""; OnPropertyChanged(); }
        }

        public string UserEmail
        {
            get => _userEmail;
            set { _userEmail = value ?? ""; OnPropertyChanged(); }
        }

        public string NewBranchName
        {
            get => _newBranchName;
            set { _newBranchName = value ?? ""; OnPropertyChanged(); }
        }

        public string SelectedBranch
        {
            get => _selectedBranch;
            set { _selectedBranch = value ?? ""; OnPropertyChanged(); }
        }

        public string CurrentBranch
        {
            get => _currentBranch;
            private set { _currentBranch = string.IsNullOrWhiteSpace(value) ? UnknownBranch : value; OnPropertyChanged(); }
        }

        public string LogText
        {
            get => _logText;
            private set
            {
                _logText = value ?? "";
                OnPropertyChanged();
                RelayCommandRaise();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BusyText));
                RelayCommandRaise();
            }
        }

        public bool HasValidRepo
        {
            get => _hasValidRepo;
            private set
            {
                if (_hasValidRepo == value)
                    return;
                _hasValidRepo = value;
                OnPropertyChanged();
                RelayCommandRaise();
            }
        }

        public string ReleaseTag
        {
            get => _releaseTag;
            set { _releaseTag = value ?? ""; OnPropertyChanged(); PersistSettings(); }
        }

        public string ReleaseTitle
        {
            get => _releaseTitle;
            set { _releaseTitle = value ?? ""; OnPropertyChanged(); PersistSettings(); }
        }

        public string ReleaseNotes
        {
            get => _releaseNotes;
            set { _releaseNotes = value ?? ""; OnPropertyChanged(); PersistSettings(); }
        }

        public string ReleaseTargetBranch
        {
            get => _releaseTargetBranch;
            set { _releaseTargetBranch = value ?? ""; OnPropertyChanged(); }
        }

        public bool ReleaseLatest
        {
            get => _releaseLatest;
            set
            {
                if (_releaseLatest == value)
                    return;

                _releaseLatest = value;
                if (value && _releasePrerelease)
                {
                    _releasePrerelease = false;
                    OnPropertyChanged(nameof(ReleasePrerelease));
                }

                OnPropertyChanged();
            }
        }

        public bool ReleasePrerelease
        {
            get => _releasePrerelease;
            set
            {
                if (_releasePrerelease == value)
                    return;

                _releasePrerelease = value;
                if (value && _releaseLatest)
                {
                    _releaseLatest = false;
                    OnPropertyChanged(nameof(ReleaseLatest));
                }

                OnPropertyChanged();
            }
        }

        public ICommand BrowseRepoCommand { get; }
        public ICommand CreateNewRepoCommand { get; }
        public ICommand SaveRepoCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenGitHubCommand { get; }
        public ICommand RefreshStatusCommand { get; }
        public ICommand PullCommand { get; }
        public ICommand DiffCommand { get; }
        public ICommand AddAllCommand { get; }
        public ICommand CommitCommand { get; }
        public ICommand PushCommand { get; }
        public ICommand ClearWorkflowCommand { get; }
        public ICommand CreateReleaseCommand { get; }
        public ICommand OpenReleasesCommand { get; }
        public ICommand LoadGitConfigCommand { get; }
        public ICommand ApplyGitConfigCommand { get; }
        public ICommand ClearIdentityCommand { get; }
        public ICommand CreateBranchCommand { get; }
        public ICommand CheckoutBranchCommand { get; }
        public ICommand PushBranchCommand { get; }
        public ICommand ClearLogCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnLanguageChanged()
        {
            RefreshThemeOptions();
            Ui.NotifyAllProperties();
            OnPropertyChanged(nameof(BusyText));
            OnPropertyChanged(nameof(SelectedLanguage));
            LanguageChanged?.Invoke();
        }

        /// <summary>Raised when UI strings should refresh (e.g. tab headers).</summary>
        public event Action LanguageChanged;

        private void RefreshThemeOptions()
        {
            var selected = SelectedThemeId;
            Themes.Clear();
            Themes.Add(new ThemeOption(ThemeManager.Light, Loc.Get("Theme_Light")));
            Themes.Add(new ThemeOption(ThemeManager.Dark, Loc.Get("Theme_Dark")));
            if (Themes.All(t => t.Id != selected))
                selected = ThemeManager.Light;
            _selectedThemeId = selected;
            OnPropertyChanged(nameof(SelectedThemeId));
            OnPropertyChanged(nameof(SelectedTheme));
        }

        private void PersistSettings()
        {
            if (!_persistSettingsEnabled)
                return;

            _settings.Language = SelectedLanguageCode;
            _settings.Theme = SelectedThemeId;
            _settings.ConfirmCommit = ConfirmCommit;
            _settings.AutoRefreshOnSaveRepo = AutoRefreshOnSaveRepo;
            _settings.LastCommitMessage = CommitMessage;
            _settings.LastReleaseTag = ReleaseTag;
            _settings.LastReleaseTitle = ReleaseTitle;
            _settings.LastReleaseNotes = ReleaseNotes;
            SettingsStore.Save(_settings);
        }

        private void ClearLog() => LogText = "";

        private string Caption => Loc.Get("AppTitle");

        private void BrowseRepo()
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Loc.Get("Dlg_FolderBrowse"),
                SelectedPath = Directory.Exists(RepoPath) ? RepoPath : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                RepoPath = dlg.SelectedPath;
        }

        private async Task CreateNewRepoAsync()
        {
            if (!GitHubCliRunner.IsAvailable())
            {
                MessageBox.Show(Loc.Get("Msg_NewRepo_GhRequired"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var parentHint = Directory.Exists(RepoPath)
                ? (Directory.Exists(Path.Combine(RepoPath, ".git"))
                    ? Path.GetDirectoryName(RepoPath)
                    : RepoPath)
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var dialog = new NewRepositoryDialog(parentHint, SelectedThemeId)
            {
                Owner = Application.Current?.MainWindow,
            };

            if (dialog.ShowDialog() != true || dialog.Result == null)
                return;

            var request = dialog.Result;
            var path = Path.GetFullPath(request.FullPath);

            if (Directory.Exists(path) && GitRunner.IsGitRepository(path))
            {
                if (MessageBox.Show(
                        Loc.Get("Msg_FolderAlreadyGit"),
                        Caption,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                RepoPath = path;
                FinishNewRepoSetup(path);
                await RefreshStatusAsync();
                return;
            }

            IsBusy = true;
            _operationCts?.Cancel();
            _operationCts = new CancellationTokenSource();
            var token = _operationCts.Token;
            try
            {
                if (!await GitHubCliRunner.IsAuthenticatedAsync(token).ConfigureAwait(true))
                {
                    MessageBox.Show(Loc.Get("Msg_GhNotAuthenticated"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    await RepoScaffoldService.ApplyAsync(request, UserName, token).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    AppendLog(Loc.Get("Msg_NewRepo_ScaffoldFailed") + " " + ex.Message, true);
                    return;
                }

                var init = await RunGitInDirectoryLoggedAsync(path, token, "init", "-b", "main");
                if (!init.Success)
                {
                    init = await RunGitInDirectoryLoggedAsync(path, token, "init");
                    if (!init.Success)
                        return;
                    await RunGitInDirectoryLoggedAsync(path, token, "branch", "-M", "main");
                }

                AppendLog(string.Format(Loc.Get("Msg_RepoInitialized"), path));

                if (!await ConfigureLocalIdentityForRepoAsync(path, token))
                    return;

                if (!await EnsureInitialCommitAsync(path, token))
                {
                    MessageBox.Show(Loc.Get("Msg_NewRepo_NoCommits"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AppendLog(Loc.Get("Msg_NewRepo_CreatingGitHub"));
                var gh = await GitHubCliRunner.CreateRepositoryAsync(request, token).ConfigureAwait(true);
                LogResult(gh, "gh repo create");
                if (!gh.Success)
                {
                    MessageBox.Show(
                        gh.StandardError + "\n\n" + Loc.Get("Msg_NewRepo_GhFailedHint"),
                        Caption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    RepoPath = path;
                    FinishNewRepoSetup(path);
                    await RefreshStatusAsync();
                    return;
                }

                AppendLog(string.Format(Loc.Get("Msg_NewRepo_GitHubCreated"), request.Name));
                var webUrl = await GitHubCliRunner.TryGetRemoteWebUrlAsync(path, token).ConfigureAwait(true);
                if (!string.IsNullOrWhiteSpace(webUrl))
                {
                    _settings.CachedGitHubUrl = webUrl;
                    SettingsStore.Save(_settings);
                }

                RepoPath = path;
                FinishNewRepoSetup(path);

                if (MessageBox.Show(
                        Loc.Get("Msg_AddFolderNow"),
                        Caption,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await CopyFolderIntoRepoAsync(path);
                    if (await EnsureOriginRemoteAsync())
                    {
                        var pushBranch = await ResolveCurrentBranchNameAsync(path, token);
                        await RunGitInDirectoryLoggedAsync(path, token, "push", "-u", "origin", pushBranch);
                    }
                }

                await RefreshStatusAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> ConfigureLocalIdentityForRepoAsync(string path, CancellationToken token)
        {
            var name = UserName?.Trim();
            var email = UserEmail?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                var globalName = await RunGitInDirectoryAsync(path, token, "config", "--global", "user.name");
                if (globalName.Success)
                    name = globalName.StandardOutput.Trim();
            }

            if (string.IsNullOrEmpty(email))
            {
                var globalEmail = await RunGitInDirectoryAsync(path, token, "config", "--global", "user.email");
                if (globalEmail.Success)
                    email = globalEmail.StandardOutput.Trim();
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show(Loc.Get("Msg_NeedIdentityForCommit"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var setName = await RunGitInDirectoryLoggedAsync(path, token, "config", "user.name", name);
            if (!setName.Success)
                return false;

            var setEmail = await RunGitInDirectoryLoggedAsync(path, token, "config", "user.email", email);
            return setEmail.Success;
        }

        private async Task<bool> EnsureInitialCommitAsync(string path, CancellationToken token)
        {
            var head = await RunGitInDirectoryAsync(path, token, "rev-parse", "HEAD");
            if (head.Success)
                return true;

            var add = await RunGitInDirectoryLoggedAsync(path, token, "add", "-A");
            if (!add.Success)
                return false;

            var commitMsg = Loc.GetEnglish("Msg_InitialCommit");
            var commit = await RunGitInDirectoryLoggedAsync(path, token, "commit", "-m", commitMsg);
            if (commit.Success)
                return true;

            var combined = commit.StandardOutput + commit.StandardError;
            if (IsNothingToCommitMessage(combined))
            {
                commit = await RunGitInDirectoryLoggedAsync(path, token, "commit", "--allow-empty", "-m", Loc.GetEnglish("Msg_InitialCommit"));
                if (commit.Success)
                    return true;
            }

            if (combined.IndexOf("user.name", StringComparison.OrdinalIgnoreCase) >= 0 ||
                combined.IndexOf("user.email", StringComparison.OrdinalIgnoreCase) >= 0 ||
                combined.IndexOf("who you are", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageBox.Show(Loc.Get("Msg_NeedIdentityForCommit"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return false;
        }

        private static bool IsNothingToCommitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.IndexOf("nothing to commit", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("nothing added to commit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void FinishNewRepoSetup(string path)
        {
            SettingsStore.RememberRepo(_settings, path);
            if (!RecentRepoPaths.Contains(path))
                RecentRepoPaths.Insert(0, path);
            PersistSettings();
            ValidateRepo();
        }

        private async Task CopyFolderIntoRepoAsync(string repoPath)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Loc.Get("Dlg_PickFolderToCopy"),
                ShowNewFolderButton = false,
            };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var sourceDir = Path.GetFullPath(dlg.SelectedPath.Trim());
            if (string.Equals(sourceDir.TrimEnd('\\', '/'), repoPath.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(Loc.Get("Msg_CopySameFolder"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int copied;
            try
            {
                copied = RepoFileCopy.CopyDirectoryContents(sourceDir, repoPath);
            }
            catch (Exception ex)
            {
                AppendLog(Loc.Get("Msg_CopyFolderFailed") + " " + ex.Message, true);
                return;
            }

            if (copied <= 0)
            {
                AppendLog(Loc.Get("Msg_CopyFolderEmpty"), true);
                return;
            }

            AppendLog(string.Format(Loc.Get("Msg_FolderCopied"), copied));
            await RunGitInDirectoryLoggedAsync(repoPath, "add", "-A");
            var commit = await RunGitInDirectoryLoggedAsync(repoPath, "commit", "-m", Loc.GetEnglish("Msg_AddFilesCommit"));
            if (!commit.Success)
            {
                var combined = commit.StandardOutput + commit.StandardError;
                if (combined.IndexOf("nothing to commit", StringComparison.OrdinalIgnoreCase) < 0)
                    return;
            }

            await RefreshStatusAsync();
        }

        private async Task<bool> EnsureOriginRemoteAsync()
        {
            var currentUrl = "";
            var remote = await RunGitQuietAsync("remote", "get-url", "origin");
            if (remote.Success && !string.IsNullOrWhiteSpace(remote.StandardOutput))
                currentUrl = remote.StandardOutput.Trim();

            var defaultUrl = string.IsNullOrWhiteSpace(currentUrl)
                ? "https://github.com/user/repo.git"
                : currentUrl;

            var url = Microsoft.VisualBasic.Interaction.InputBox(
                Loc.Get("Dlg_RemoteUrlPrompt"),
                Loc.Get("Dlg_ConfirmRemoteTitle"),
                defaultUrl);

            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.Trim();
            if (!string.IsNullOrEmpty(currentUrl) &&
                string.Equals(url, currentUrl, StringComparison.OrdinalIgnoreCase))
                return true;

            GitRunResult result;
            if (string.IsNullOrEmpty(currentUrl))
            {
                AppendLog(Loc.Get("Msg_NoOrigin"), true);
                result = await RunGitLoggedAsync("remote", "add", "origin", url);
                if (result.Success)
                    AppendLog(string.Format(Loc.Get("Msg_RemoteAdded"), url));
            }
            else
            {
                result = await RunGitLoggedAsync("remote", "set-url", "origin", url);
                if (result.Success)
                    AppendLog(string.Format(Loc.Get("Msg_RemoteUpdated"), url));
            }

            return result.Success;
        }

        private void SaveRepo()
        {
            ValidateRepo();
            if (!HasValidRepo)
            {
                MessageBox.Show(Loc.Get("Msg_NoGitFolder"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SettingsStore.RememberRepo(_settings, RepoPath);
            PersistSettings();

            if (!RecentRepoPaths.Contains(RepoPath))
                RecentRepoPaths.Insert(0, RepoPath);

            AppendLog(string.Format(Loc.Get("Msg_SavedRepo"), RepoPath));
            if (AutoRefreshOnSaveRepo)
                _ = RefreshStatusAsync();
        }

        private void OpenFolder()
        {
            if (!HasValidRepo)
                return;
            Process.Start("explorer.exe", RepoPath);
        }

        private async Task OpenGitHubAsync()
        {
            var url = _settings.CachedGitHubUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                var remote = await RunGitLoggedAsync("remote", "get-url", "origin");
                if (remote.Success)
                {
                    url = GitRunner.ToGitHubWebUrl(remote.StandardOutput.Trim());
                    _settings.CachedGitHubUrl = url ?? "";
                    SettingsStore.Save(_settings);
                }
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(Loc.Get("Msg_NoGitHub"), Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private async Task RefreshStatusAsync()
        {
            if (!HasValidRepo)
                return;

            var branch = await RunGitLoggedAsync("branch", "--show-current");
            if (branch.Success && !string.IsNullOrWhiteSpace(branch.StandardOutput))
                CurrentBranch = branch.StandardOutput.Trim();

            await RunGitLoggedAsync("status");
            await RefreshBranchesAsync();
        }

        private async Task RefreshBranchesAsync()
        {
            var result = await RunGitQuietAsync("branch", "--format=%(refname:short)");
            Branches.Clear();
            if (!result.Success)
                return;

            foreach (var line in result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var b = line.Trim();
                if (!string.IsNullOrEmpty(b))
                    Branches.Add(b);
            }

            if (Branches.Count > 0 && string.IsNullOrWhiteSpace(SelectedBranch))
                SelectedBranch = CurrentBranch != UnknownBranch ? CurrentBranch : Branches[0];
        }

        private async Task PullAsync()
        {
            var branch = CurrentBranch;
            if (branch == UnknownBranch || string.IsNullOrWhiteSpace(branch))
                branch = "main";

            var result = await RunGitLoggedAsync("pull", "--rebase", "origin", branch);
            if (!result.Success)
                await RunGitLoggedAsync("pull", "origin", branch);

            await RefreshStatusAsync();
        }

        private async Task DiffAsync() => await RunGitLoggedAsync("diff");

        private async Task AddAllAsync()
        {
            await RunGitLoggedAsync("add", "-A");
            await RefreshStatusAsync();
        }

        private async Task CommitAsync()
        {
            if (string.IsNullOrWhiteSpace(CommitMessage))
            {
                MessageBox.Show(Loc.Get("Msg_EnterCommit"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ConfirmCommit &&
                MessageBox.Show(Loc.Get("Msg_ConfirmCommit"), Loc.Get("Dlg_Commit"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _settings.LastCommitMessage = CommitMessage;
            SettingsStore.Save(_settings);

            await RunGitLoggedAsync("commit", "-m", CommitMessage);
            await RefreshStatusAsync();
        }

        private async Task PushAsync()
        {
            var branch = CurrentBranch;
            if (branch == UnknownBranch || string.IsNullOrWhiteSpace(branch))
            {
                MessageBox.Show(Loc.Get("Msg_NoBranch"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await EnsureOriginRemoteAsync())
                return;

            if (await HasUncommittedChangesAsync())
            {
                MessageBox.Show(
                    Loc.Get("Msg_PushNeedCommit"),
                    Caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var push = await RunGitLoggedAsync("push", "-u", "origin", branch);
            if (!push.Success)
            {
                var err = push.StandardOutput + push.StandardError;
                if (IsNonFastForwardPushError(err))
                {
                    MessageBox.Show(
                        Loc.Get("Msg_PushNeedPull"),
                        Caption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        Loc.Get("Msg_PushFailedHint"),
                        Caption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }

            await RefreshStatusAsync();
        }

        private async Task<bool> HasUncommittedChangesAsync()
        {
            var status = await RunGitQuietAsync("status", "--porcelain");
            return status.Success && !string.IsNullOrWhiteSpace(status.StandardOutput);
        }

        private static bool IsNonFastForwardPushError(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.IndexOf("non-fast-forward", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("failed to push some refs", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Updates were rejected", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ClearWorkflow()
        {
            if (MessageBox.Show(
                    Loc.Get("Msg_ConfirmClearWorkflow"),
                    Loc.Get("Dlg_ClearWorkflow"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            RepoPath = "";
            CommitMessage = "";
            NewBranchName = "";
            SelectedBranch = "";
            Branches.Clear();
            CurrentBranch = UnknownBranch;
            _settings.RepoPath = "";
            _settings.LastCommitMessage = "";
            PersistSettings();
        }

        private async Task CreateReleaseAsync()
        {
            if (!GitHubCliRunner.IsAvailable())
            {
                MessageBox.Show(Loc.Get("Msg_ReleaseGhRequired"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ReleaseTag))
            {
                MessageBox.Show(Loc.Get("Msg_ReleaseTagRequired"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var target = ReleaseTargetBranch;
            if (string.IsNullOrWhiteSpace(target) && CurrentBranch != UnknownBranch)
                target = CurrentBranch;

            if (ReleaseLatest && ReleasePrerelease)
            {
                MessageBox.Show(Loc.Get("Msg_ReleaseLatestPrereleaseConflict"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var request = new ReleaseRequest
            {
                Tag = ReleaseTag.Trim(),
                Title = ReleaseTitle?.Trim(),
                Notes = ReleaseNotes?.Trim(),
                TargetBranch = string.IsNullOrWhiteSpace(target) ? null : target.Trim(),
                IsLatest = ReleaseLatest,
                IsPrerelease = ReleasePrerelease,
            };

            IsBusy = true;
            try
            {
                var result = await GitHubCliRunner.CreateReleaseAsync(RepoPath, request, CancellationToken.None)
                    .ConfigureAwait(true);
                LogResult(result, "gh release create " + request.Tag);
                if (result.Success)
                    MessageBox.Show(
                        string.Format(Loc.Get("Msg_ReleaseCreated"), request.Tag),
                        Caption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OpenReleasesPageAsync()
        {
            var url = _settings.CachedGitHubUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                var remote = await RunGitQuietAsync("remote", "get-url", "origin");
                if (remote.Success)
                    url = GitRunner.ToGitHubWebUrl(remote.StandardOutput.Trim());
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                var ghUrl = await GitHubCliRunner.TryGetRemoteWebUrlAsync(RepoPath, CancellationToken.None)
                    .ConfigureAwait(true);
                if (!string.IsNullOrWhiteSpace(ghUrl))
                    url = ghUrl;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(Loc.Get("Msg_NoGitHub"), Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            url = url.Trim().TrimEnd('/') + "/releases";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private async Task LoadGitConfigAsync()
        {
            var name = await RunGitQuietAsync("config", "--global", "user.name");
            var email = await RunGitQuietAsync("config", "--global", "user.email");

            UserName = name.Success ? name.StandardOutput.Trim() : "";
            UserEmail = email.Success ? email.StandardOutput.Trim() : "";

            AppendLog(Loc.Get("Msg_LoadedConfig"));
        }

        private async Task ApplyGitConfigAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) && string.IsNullOrWhiteSpace(UserEmail))
            {
                MessageBox.Show(Loc.Get("Msg_EnterNameEmail"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(UserName))
                await RunGitLoggedAsync("config", "--global", "user.name", UserName.Trim());

            if (!string.IsNullOrWhiteSpace(UserEmail))
                await RunGitLoggedAsync("config", "--global", "user.email", UserEmail.Trim());
        }

        private async Task ClearGitIdentityAsync()
        {
            if (MessageBox.Show(
                    Loc.Get("Msg_ConfirmClearIdentity"),
                    Loc.Get("Dlg_ClearIdentity"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            await RunGitQuietAsync("config", "--global", "--unset", "user.name");
            await RunGitQuietAsync("config", "--global", "--unset", "user.email");

            UserName = "";
            UserEmail = "";
            AppendLog(Loc.Get("Msg_IdentityCleared"));
        }

        private async Task CreateBranchAsync()
        {
            if (string.IsNullOrWhiteSpace(NewBranchName))
            {
                MessageBox.Show(Loc.Get("Msg_EnterBranchName"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await RunGitLoggedAsync("checkout", "-b", NewBranchName.Trim());
            CurrentBranch = NewBranchName.Trim();
            await RefreshStatusAsync();
        }

        private async Task CheckoutBranchAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedBranch))
                return;

            await RunGitLoggedAsync("checkout", SelectedBranch.Trim());
            await RefreshStatusAsync();
        }

        private async Task PushBranchAsync()
        {
            var branch = !string.IsNullOrWhiteSpace(SelectedBranch) ? SelectedBranch.Trim() : CurrentBranch;
            if (branch == UnknownBranch || string.IsNullOrWhiteSpace(branch))
            {
                MessageBox.Show(Loc.Get("Msg_SelectBranch"), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await EnsureOriginRemoteAsync())
                return;

            await RunGitLoggedAsync("push", "-u", "origin", branch);
            await RefreshStatusAsync();
        }

        private async Task<string> ResolveCurrentBranchNameAsync(string repoPath, CancellationToken token)
        {
            var branch = await RunGitInDirectoryAsync(repoPath, token, "branch", "--show-current").ConfigureAwait(false);
            if (branch.Success && !string.IsNullOrWhiteSpace(branch.StandardOutput))
                return branch.StandardOutput.Trim();

            return "main";
        }

        private void ValidateRepo()
        {
            HasValidRepo = GitRunner.IsGitRepository(RepoPath);
        }

        private Task<GitRunResult> RunGitInDirectoryLoggedAsync(string workDir, params string[] args) =>
            RunGitInDirectoryLoggedAsync(workDir, CancellationToken.None, args);

        private async Task<GitRunResult> RunGitInDirectoryLoggedAsync(
            string workDir,
            CancellationToken cancellationToken,
            params string[] args)
        {
            var result = await RunGitInDirectoryAsync(workDir, cancellationToken, args).ConfigureAwait(true);
            LogResult(result, string.Join(" ", args));
            return result;
        }

        private Task<GitRunResult> RunGitInDirectoryAsync(string workDir, params string[] args) =>
            RunGitInDirectoryAsync(workDir, CancellationToken.None, args);

        private async Task<GitRunResult> RunGitInDirectoryAsync(
            string workDir,
            CancellationToken cancellationToken,
            params string[] args)
        {
            if (string.IsNullOrWhiteSpace(workDir) || !Directory.Exists(workDir))
                return new GitRunResult { ExitCode = -1, StandardError = Loc.Get("Msg_NoValidRepo") };

            var manageBusy = !IsBusy;
            if (manageBusy)
            {
                IsBusy = true;
                _operationCts?.Cancel();
                _operationCts = new CancellationTokenSource();
            }

            var token = cancellationToken.CanBeCanceled
                ? cancellationToken
                : (_operationCts?.Token ?? CancellationToken.None);

            try
            {
                return await GitRunner.RunAsync(workDir, token, args).ConfigureAwait(true);
            }
            finally
            {
                if (manageBusy)
                    IsBusy = false;
            }
        }

        private async Task<GitRunResult> RunGitLoggedAsync(params string[] args)
        {
            var result = await RunGitQuietAsync(args);
            LogResult(result, string.Join(" ", args));
            return result;
        }

        private async Task<GitRunResult> RunGitQuietAsync(params string[] args)
        {
            var isConfig = args.Length > 0 && args[0].Equals("config", StringComparison.OrdinalIgnoreCase);
            var isGlobalConfig = isConfig && args.Any(a => a.Equals("--global", StringComparison.OrdinalIgnoreCase));

            if (!HasValidRepo && !isGlobalConfig)
                return new GitRunResult { ExitCode = -1, StandardError = Loc.Get("Msg_NoValidRepo") };

            var workDir = HasValidRepo ? RepoPath : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            IsBusy = true;
            _operationCts?.Cancel();
            _operationCts = new CancellationTokenSource();
            try
            {
                return await GitRunner.RunAsync(workDir, _operationCts.Token, args).ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LogResult(GitRunResult result, string commandLabel)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var header = $"[{time}] git {commandLabel} (exit {result.ExitCode})";
            var body = result.StandardOutput;
            if (!string.IsNullOrWhiteSpace(result.StandardError))
                body = (string.IsNullOrEmpty(body) ? "" : body + Environment.NewLine) + result.StandardError;

            var isError = !result.Success;
            AppendLog(header + Environment.NewLine + body, isError);
        }

        private void AppendLog(string text, bool isError = false)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var prefix = isError ? "[ERROR] " : "";
            LogText = LogText + (LogText.Length > 0 ? Environment.NewLine + Environment.NewLine : "") + prefix + text;
        }

        private void RelayCommandRaise() => System.Windows.Input.CommandManager.InvalidateRequerySuggested();

        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

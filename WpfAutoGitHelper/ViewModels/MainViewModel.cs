using System;
using System.Collections.Generic;
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
    public sealed partial class MainViewModel : INotifyPropertyChanged
    {
        private const string UnknownBranch = "-";

        private readonly AppSettings _settings;
        private readonly GitOperationsService _gitOps = new GitOperationsService();
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
        private string _selectedAccentId = AccentPalette.DefaultId;
        private string _selectedBackgroundId = BackgroundPalette.DefaultId;
        private bool _confirmCommit = true;
        private bool _showFieldHints = true;
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
            _selectedAccentId = AccentPalette.NormalizeId(_settings.AccentColor);
            _selectedBackgroundId = BackgroundPalette.NormalizeId(_settings.BackgroundColor);
            ConfirmCommit = _settings.ConfirmCommit;
            ShowFieldHints = _settings.ShowFieldHints;
            AutoRunCreateGithubRepo = _settings.AutoRunCreateGithubRepo;
            AutoRefreshOnSaveRepo = _settings.AutoRefreshOnSaveRepo;
            ReleaseTag = _settings.LastReleaseTag ?? "";
            ReleaseTitle = _settings.LastReleaseTitle ?? "";
            ReleaseNotes = _settings.LastReleaseNotes ?? "";
            foreach (var assetPath in _settings.LastReleaseAssetPaths ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(assetPath) && File.Exists(assetPath) && !ReleaseAssets.Contains(assetPath))
                    ReleaseAssets.Add(assetPath);
            }

            foreach (var p in _settings.RecentRepoPaths ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(p) && !RecentRepoPaths.Contains(p))
                    RecentRepoPaths.Add(p);
            }

            Loc.LanguageChanged += OnLanguageChanged;
            _selectedLanguageCode = Loc.Normalize(_settings.Language);
            Loc.ApplyLanguage(_selectedLanguageCode);
            RefreshThemeOptions();
            RefreshAccentOptions();
            RefreshBackgroundOptions();
            ApplyAppearance();
            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(SelectedTheme));
            OnPropertyChanged(nameof(SelectedAccent));
            OnPropertyChanged(nameof(SelectedBackground));

            _gitOps.ResolveWorkingDirectory = () => HasValidRepo ? RepoPath : null;
            _gitOps.SetBusy = SetBusyFromGitCommand;
            _gitOps.IsBusySuppressed = () => _suppressGitCommandBusy;
            _gitOps.GetCancellationToken = () => _operationCts?.Token ?? CancellationToken.None;
            _gitOps.LogResult = (result, commandLabel) => LogResult(result, commandLabel);

            BrowseRepoCommand = new RelayCommand(BrowseRepo);
            CreateNewRepoCommand = new RelayCommand(async () => await CreateNewRepoAsync(), () => !IsBusy);
            SaveRepoCommand = new RelayCommand(async () => await SaveRepoAsync(), () => !IsBusy);
            OpenFolderCommand = new RelayCommand(OpenFolder, () => HasValidRepo && !IsBusy);
            OpenGitHubCommand = new RelayCommand(async () => await OpenGitHubAsync(), () => HasValidRepo && !IsBusy);
            RefreshStatusCommand = new RelayCommand(async () => await RefreshStatusAsync(), () => HasValidRepo && !IsBusy);
            PullCommand = new RelayCommand(async () => await PullAsync(), () => HasValidRepo && !IsBusy);
            DiffCommand = new RelayCommand(async () => await DiffAsync(), () => HasValidRepo && !IsBusy);
            AddAllCommand = new RelayCommand(async () => await AddAllAsync(), () => HasValidRepo && !IsBusy);
            CommitCommand = new RelayCommand(async () => await CommitAsync(), () => HasValidRepo && !IsBusy);
            PushCommand = new RelayCommand(async () => await PushAsync(), () => HasValidRepo && !IsBusy);
            SyncToGitHubCommand = new RelayCommand(async () => await SyncToGitHubAsync(), () => HasValidRepo && !IsBusy);
            ConfigureOriginCommand = new RelayCommand(async () => await ConfigureOriginAsync(), () => HasValidRepo && !IsBusy);
            ClearWorkflowCommand = new RelayCommand(async () => await ClearWorkflowAsync(), () => !IsBusy);
            CreateReleaseCommand = new RelayCommand(async () => await CreateReleaseAsync(), () => HasValidRepo && !IsBusy);
            OpenReleasesCommand = new RelayCommand(async () => await OpenReleasesPageAsync(), () => HasValidRepo && !IsBusy);
            AddReleaseAssetsCommand = new RelayCommand(AddReleaseAssets, () => !IsBusy);
            AddReleaseImagesCommand = new RelayCommand(AddReleaseImages, () => !IsBusy);
            RemoveReleaseAssetCommand = new RelayCommand(RemoveSelectedReleaseAsset, () => !IsBusy && SelectedReleaseAsset != null);
            AddReleaseBuildOutputCommand = new RelayCommand(async () => await AddReleaseBuildOutputAsync(), () => HasValidRepo && !IsBusy);
            LoadGitConfigCommand = new RelayCommand(async () => await LoadGitConfigAsync(), () => !IsBusy);
            ApplyGitConfigCommand = new RelayCommand(async () => await ApplyGitConfigAsync(), () => !IsBusy);
            ClearIdentityCommand = new RelayCommand(async () => await ClearGitIdentityAsync(), () => !IsBusy);
            CreateBranchCommand = new RelayCommand(async () => await CreateBranchAsync(), () => HasValidRepo && !IsBusy);
            CheckoutBranchCommand = new RelayCommand(async () => await CheckoutBranchAsync(), () => HasValidRepo && !IsBusy && !string.IsNullOrWhiteSpace(SelectedBranch));
            PushBranchCommand = new RelayCommand(async () => await PushBranchAsync(), () => HasValidRepo && !IsBusy);
            ClearLogCommand = new RelayCommand(ClearLog, () => !string.IsNullOrEmpty(LogText));

            InitAppDialogCommands();
            InitOriginRemote();
            InitUiMode();
            InitAdvanced();

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
        public ObservableCollection<AccentColorOption> AccentColors { get; } = new ObservableCollection<AccentColorOption>();
        public ObservableCollection<BackgroundColorOption> BackgroundColors { get; } = new ObservableCollection<BackgroundColorOption>();

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
        public ObservableCollection<string> ReleaseAssets { get; } = new ObservableCollection<string>();

        private string _selectedReleaseAsset;

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
                OnPropertyChanged(nameof(SpellCheckLanguageTag));
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
                OnPropertyChanged(nameof(SpellCheckLanguageTag));
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

                var themeId = ThemeManager.Normalize(value.Id);
                if (string.Equals(_selectedThemeId, themeId, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedThemeId = themeId;
                ApplyAppearance();
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
                var theme = ThemeManager.Normalize(value);
                if (string.Equals(_selectedThemeId, theme, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedThemeId = theme;
                ApplyAppearance();
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedTheme));
                PersistSettings();
            }
        }

        public AccentColorOption SelectedAccent
        {
            get
            {
                foreach (var accent in AccentColors)
                {
                    if (string.Equals(accent.Id, _selectedAccentId, StringComparison.OrdinalIgnoreCase))
                        return accent;
                }

                return AccentColors.Count > 0 ? AccentColors[0] : null;
            }
            set
            {
                if (value == null)
                    return;

                var id = AccentPalette.NormalizeId(value.Id);
                if (string.Equals(_selectedAccentId, id, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedAccentId = id;
                ApplyAppearance();
                OnPropertyChanged(nameof(SelectedAccentId));
                OnPropertyChanged(nameof(SelectedAccent));
                PersistSettings();
            }
        }

        public string SelectedAccentId
        {
            get => _selectedAccentId;
            set
            {
                var id = AccentPalette.NormalizeId(value);
                if (string.Equals(_selectedAccentId, id, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedAccentId = id;
                ApplyAppearance();
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAccent));
                PersistSettings();
            }
        }

        public BackgroundColorOption SelectedBackground
        {
            get
            {
                foreach (var bg in BackgroundColors)
                {
                    if (string.Equals(bg.Id, _selectedBackgroundId, StringComparison.OrdinalIgnoreCase))
                        return bg;
                }

                return BackgroundColors.Count > 0 ? BackgroundColors[0] : null;
            }
            set
            {
                if (value == null)
                    return;

                var id = BackgroundPalette.NormalizeId(value.Id);
                if (string.Equals(_selectedBackgroundId, id, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedBackgroundId = id;
                ApplyAppearance();
                OnPropertyChanged(nameof(SelectedBackgroundId));
                OnPropertyChanged(nameof(SelectedBackground));
                PersistSettings();
            }
        }

        public string SelectedBackgroundId
        {
            get => _selectedBackgroundId;
            set
            {
                var id = BackgroundPalette.NormalizeId(value);
                if (string.Equals(_selectedBackgroundId, id, StringComparison.OrdinalIgnoreCase))
                    return;

                _selectedBackgroundId = id;
                ApplyAppearance();
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedBackground));
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

        public bool ShowFieldHints
        {
            get => _showFieldHints;
            set
            {
                if (_showFieldHints == value)
                    return;
                _showFieldHints = value;
                OnPropertyChanged();
                PersistSettings();
            }
        }

        public string BusyText => string.Format(Loc.Get("Busy_Format"), IsBusy);
        public string SpellCheckLanguageTag => ToSpellCheckLanguageTag(SelectedLanguageCode);
        public string SelectedProjectName => HasValidRepo ? Path.GetFileName(RepoPath.TrimEnd('\\', '/')) : UnknownBranch;
        public string SelectedProjectFolder => HasValidRepo ? RepoPath : UnknownBranch;

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
                OnPropertyChanged(nameof(SelectedProjectName));
                OnPropertyChanged(nameof(SelectedProjectFolder));
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
                OnPropertyChanged(nameof(SelectedProjectName));
                OnPropertyChanged(nameof(SelectedProjectFolder));
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

        public string SelectedReleaseAsset
        {
            get => _selectedReleaseAsset;
            set
            {
                if (_selectedReleaseAsset == value)
                    return;
                _selectedReleaseAsset = value;
                OnPropertyChanged();
                RelayCommandRaise();
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
        public ICommand SyncToGitHubCommand { get; }
        public ICommand ConfigureOriginCommand { get; }
        public ICommand ClearWorkflowCommand { get; }
        public ICommand CreateReleaseCommand { get; }
        public ICommand OpenReleasesCommand { get; }
        public ICommand AddReleaseAssetsCommand { get; }
        public ICommand AddReleaseImagesCommand { get; }
        public ICommand RemoveReleaseAssetCommand { get; }
        public ICommand AddReleaseBuildOutputCommand { get; }
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
            RefreshAccentOptions();
            RefreshBackgroundOptions();
            Ui.NotifyAllProperties();
            NotifyAdvUiLabels();
            RebuildBranchPickerLists();
            OnPropertyChanged(nameof(BusyText));
            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(SpellCheckLanguageTag));
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
            Themes.Add(new ThemeOption(ThemeManager.Black, Loc.Get("Theme_Black")));
            if (Themes.All(t => t.Id != selected))
                selected = ThemeManager.Light;
            _selectedThemeId = selected;
            OnPropertyChanged(nameof(SelectedThemeId));
            OnPropertyChanged(nameof(SelectedTheme));
        }

        private void RefreshAccentOptions()
        {
            var selected = SelectedAccentId;
            AccentColors.Clear();
            foreach (var id in AccentPalette.AllIds)
                AccentColors.Add(new AccentColorOption(id, Loc.Get(GetAccentLocKey(id))));

            if (AccentColors.All(a => a.Id != selected))
                selected = AccentPalette.DefaultId;
            _selectedAccentId = selected;
            OnPropertyChanged(nameof(SelectedAccentId));
            OnPropertyChanged(nameof(SelectedAccent));
        }

        private static string GetAccentLocKey(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "Accent_Blue";

            return "Accent_" + char.ToUpperInvariant(id[0]) + id.Substring(1);
        }

        private void RefreshBackgroundOptions()
        {
            var selected = SelectedBackgroundId;
            BackgroundColors.Clear();
            foreach (var id in BackgroundPalette.AllIds)
                BackgroundColors.Add(new BackgroundColorOption(id, Loc.Get(GetBackgroundLocKey(id))));

            if (BackgroundColors.All(b => b.Id != selected))
                selected = BackgroundPalette.DefaultId;
            _selectedBackgroundId = selected;
            OnPropertyChanged(nameof(SelectedBackgroundId));
            OnPropertyChanged(nameof(SelectedBackground));
        }

        private static string GetBackgroundLocKey(string id)
        {
            if (string.IsNullOrEmpty(id) || id == BackgroundPalette.DefaultId)
                return "Background_Default";

            return "Background_" + char.ToUpperInvariant(id[0]) + id.Substring(1);
        }

        private void ApplyAppearance()
        {
            AppearanceManager.Apply(_selectedThemeId, _selectedAccentId, _selectedBackgroundId);
        }

        private void PersistSettings()
        {
            if (!_persistSettingsEnabled)
                return;

            _settings.Language = SelectedLanguageCode;
            _settings.Theme = SelectedThemeId;
            _settings.AccentColor = SelectedAccentId;
            _settings.BackgroundColor = SelectedBackgroundId;
            _settings.ConfirmCommit = ConfirmCommit;
            _settings.ShowFieldHints = ShowFieldHints;
            _settings.AutoRunCreateGithubRepo = AutoRunCreateGithubRepo;
            _settings.AutoRefreshOnSaveRepo = AutoRefreshOnSaveRepo;
            _settings.LastCommitMessage = CommitMessage;
            _settings.LastReleaseTag = ReleaseTag;
            _settings.LastReleaseTitle = ReleaseTitle;
            _settings.LastReleaseNotes = ReleaseNotes;
            _settings.LastReleaseAssetPaths = ReleaseAssets.ToList();
            _settings.UiMode = IsAutoAdvancedMode ? "auto-advanced" : (IsAdvancedMode ? "advanced" : "easy");
            _settings.DefaultUiMode = DefaultUiModeIsAutoAdvanced ? "auto-advanced" : (DefaultUiModeIsAdvanced ? "advanced" : "easy");
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
                await NotifyAsync(Loc.Get("Msg_NewRepo_GhRequired")).ConfigureAwait(true);
                return;
            }

            var parentHint = Directory.Exists(RepoPath)
                ? (Directory.Exists(Path.Combine(RepoPath, ".git"))
                    ? Path.GetDirectoryName(RepoPath)
                    : RepoPath)
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var dialog = new NewRepositoryDialog(parentHint, SelectedThemeId, SelectedAccentId, SelectedBackgroundId)
            {
                Owner = Application.Current?.MainWindow,
            };

            if (dialog.ShowDialog() != true || dialog.Result == null)
                return;

            var request = dialog.Result;
            var path = Path.GetFullPath(request.FullPath);

            if (Directory.Exists(path) && GitRunner.IsGitRepository(path))
            {
                if (!await ConfirmAsync(Loc.Get("Msg_FolderAlreadyGit")).ConfigureAwait(true))
                    return;

                RepoPath = path;
                FinishNewRepoSetup(path);
                await RefreshStatusAsync();
                return;
            }

            await RunWithBusyAsync(async () =>
            {
            var token = _operationCts?.Token ?? CancellationToken.None;
            try
            {
                if (!await GitHubCliRunner.IsAuthenticatedAsync(token).ConfigureAwait(true))
                {
                    await NotifyAsync(Loc.Get("Msg_GhNotAuthenticated")).ConfigureAwait(true);
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
                    await NotifyAsync(Loc.Get("Msg_NewRepo_NoCommits")).ConfigureAwait(true);
                    return;
                }

                AppendLog(Loc.Get("Msg_NewRepo_CreatingGitHub"));
                var gh = await GitHubCliRunner.CreateRepositoryAsync(request, token).ConfigureAwait(true);
                LogResult(gh, "gh repo create");
                if (!gh.Success)
                {
                    await NotifyAsync(
                        gh.StandardError + "\n\n" + Loc.Get("Msg_NewRepo_GhFailedHint"),
                        isError: true).ConfigureAwait(true);
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

                if (await ConfirmAsync(Loc.Get("Msg_AddFolderNow")).ConfigureAwait(true))
                {
                    await CopyFolderIntoRepoAsync(path);
                    if (await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
                    {
                        var pushBranch = await ResolveCurrentBranchNameAsync(path, token);
                        await RunGitInDirectoryLoggedAsync(path, token, "push", "-u", "origin", pushBranch);
                    }
                }

                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                AppendLog(ex.Message, true);
            }
            }).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_NeedIdentityForCommit")).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_NeedIdentityForCommit")).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_CopySameFolder")).ConfigureAwait(true);
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

        private async Task<bool> EnsureOriginRemoteAsync(bool forcePrompt = false)
        {
            var currentUrl = "";
            var remote = await RunGitQuietAsync("remote", "get-url", "origin");
            if (remote.Success && !string.IsNullOrWhiteSpace(remote.StandardOutput))
                currentUrl = remote.StandardOutput.Trim();

            if (!forcePrompt && !string.IsNullOrEmpty(currentUrl))
            {
                var web = GitRunner.ToGitHubWebUrl(currentUrl);
                if (!string.IsNullOrWhiteSpace(web))
                {
                    _settings.CachedGitHubUrl = web;
                    SettingsStore.Save(_settings);
                }

                return true;
            }

            var defaultUrl = string.IsNullOrWhiteSpace(currentUrl)
                ? "https://github.com/user/repo.git"
                : currentUrl;

            var title = forcePrompt ? Loc.Get("Dlg_ConfirmRemoteTitle") : Loc.Get("Dlg_AddRemoteTitle");
            var url = await PromptInputAsync(Loc.Get("Dlg_RemoteUrlPrompt"), title, defaultUrl).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(url))
                return false;
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

            if (result.Success)
            {
                var web = GitRunner.ToGitHubWebUrl(url);
                if (!string.IsNullOrWhiteSpace(web))
                {
                    _settings.CachedGitHubUrl = web;
                    SettingsStore.Save(_settings);
                }
            }

            return result.Success;
        }

        private async Task ConfigureOriginAsync()
        {
            if (!HasValidRepo)
                return;

            await EnsureOriginRemoteAsync(forcePrompt: true);
            await RefreshStatusAsync();
        }

        private async Task SaveRepoAsync()
        {
            ValidateRepo();
            if (!HasValidRepo)
            {
                await NotifyAsync(Loc.Get("Msg_NoGitFolder")).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_NoGitHub")).ConfigureAwait(true);
                return;
            }

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private async Task RefreshStatusAsync()
        {
            if (!HasValidRepo)
                return;

            var resolved = await TryResolveBranchNameAsync().ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                CurrentBranch = await IsRebaseInProgressAsync().ConfigureAwait(true)
                    ? resolved + " (rebase)"
                    : resolved;
            }

            await RunGitLoggedAsync("status");
            await RefreshHasAnyCommitAsync();
            await RefreshBranchesAsync();
            await RefreshChangedFilesAsync();
            await RefreshConflictFilesAsync();
            await RefreshOriginRemoteUrlAsync();
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

            RebuildBranchPickerLists();
        }

        private async Task<bool> PullAsync()
        {
            var branch = await ResolveWorkingBranchAsync();
            if (branch == null)
                return false;

            await RunGitLoggedAsync("fetch", "origin");

            if (!HasAnyCommit)
            {
                AppendLog(Loc.Get("Auto_SkipPullNoCommits"));
                await RefreshStatusAsync();
                return true;
            }

            var remoteBranch = await GetOriginDefaultBranchAsync();
            var pullBranch = await RemoteBranchExistsAsync(branch)
                ? branch
                : remoteBranch;

            if (string.IsNullOrWhiteSpace(pullBranch) || !await RemoteBranchExistsAsync(pullBranch))
            {
                AppendLog(Loc.Get("Auto_SkipPullNoRemoteBranch"));
                await RefreshStatusAsync();
                return true;
            }

            if (await HasWorkingTreeChangesAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Auto_SkipPullDirtyTree"));
                await RefreshStatusAsync();
                return true;
            }

            await EnsureUpstreamAsync(branch, pullBranch);

            var result = await RunGitLoggedAsync("pull", "--rebase", "origin", pullBranch);
            if (!result.Success)
                result = await RunGitLoggedAsync("pull", "origin", pullBranch);

            await RefreshStatusAsync();
            if (result.Success)
                return true;

            if (IsAutoAdvancedMode)
            {
                var detail = (result.StandardOutput + result.StandardError).Trim();
                if (string.IsNullOrWhiteSpace(detail))
                    detail = Loc.Get("Auto_SkipPullFailed");
                AppendLog(detail, true);
                return true;
            }

            return false;
        }

        private async Task DiffAsync() => await RunGitLoggedAsync("diff");

        private async Task AddAllAsync()
        {
            await RunGitLoggedAsync("add", "-A");
            await RefreshStatusAsync();
        }

        private async Task<bool> CommitAsync()
        {
            if (string.IsNullOrWhiteSpace(CommitMessage))
            {
                await NotifyAsync(Loc.Get("Msg_EnterCommit")).ConfigureAwait(true);
                return false;
            }

            if (ConfirmCommit &&
                !await ConfirmAsync(Loc.Get("Msg_ConfirmCommit"), Loc.Get("Dlg_Commit")).ConfigureAwait(true))
                return false;

            _settings.LastCommitMessage = CommitMessage;
            SettingsStore.Save(_settings);

            var commit = await RunGitLoggedAsync("commit", "-m", CommitMessage);
            await RefreshStatusAsync();

            if (commit.Success)
                return true;

            var combined = commit.StandardOutput + commit.StandardError;
            if (combined.IndexOf("nothing to commit", StringComparison.OrdinalIgnoreCase) >= 0)
                await NotifyAsync(Loc.Get("Msg_NothingToCommit"), isError: true).ConfigureAwait(true);
            else
                await NotifyAsync(Loc.Get("Msg_CommitFailed"), isError: true).ConfigureAwait(true);

            return false;
        }

        private Task PushAsync() => PublishToGitHubAsync(showSuccessDialog: false);

        private Task SyncToGitHubAsync() => PublishToGitHubAsync(showSuccessDialog: true);

        private Task PublishToGitHubAsync(bool showSuccessDialog) =>
            RunWithBusyAsync(() => PublishToGitHubCoreAsync(showSuccessDialog));

        private async Task PublishToGitHubCoreAsync(bool showSuccessDialog)
        {
            var branch = await ResolveWorkingBranchAsync();
            if (branch == null)
                return;

            if (IsAutoAdvancedMode)
            {
                if (!await EnsureOriginForAutoAsync().ConfigureAwait(true))
                    return;
            }
            else if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
            {
                return;
            }

            AppendLog(Loc.Get("Msg_SyncStarting"));
            await RunGitLoggedAsync("fetch", "origin");
            await EnsureUpstreamAsync(branch);

            if (!await TryRecoverInterruptedGitOperationAsync())
            {
                await ShowPublishFailureAsync();
                await RefreshStatusAsync();
                return;
            }

            // Pull remote updates before committing so deleted-on-GitHub files (e.g. release zips) do not cause rebase conflicts.
            if (!await TryIntegrateRemoteAsync(branch))
            {
                await ShowPublishFailureAsync();
                await RefreshStatusAsync();
                return;
            }

            if (!await TryAutoCommitAllAsync())
            {
                await NotifyAsync(Loc.Get("Msg_SyncFailed"), isError: true).ConfigureAwait(true);
                await RefreshStatusAsync();
                return;
            }

            await RunGitLoggedAsync("fetch", "origin");

            if (!await TryIntegrateRemoteAsync(branch))
            {
                await ShowPublishFailureAsync();
                await RefreshStatusAsync();
                return;
            }

            var push = await TryPushWithRetryAsync(branch);
            if (!push.Success)
            {
                await ShowPublishFailureAsync();
                await RefreshStatusAsync();
                return;
            }

            AppendLog(Loc.Get("Msg_SyncSuccess"));
            if (showSuccessDialog)
                await NotifyAsync(Loc.Get("Msg_SyncSuccess")).ConfigureAwait(true);

            await RefreshStatusAsync();
        }

        private async Task ShowPublishFailureAsync()
        {
            if (await HasMergeConflictsInTreeAsync())
            {
                await NotifyAsync(Loc.Get("Msg_SyncConflict"), isError: true).ConfigureAwait(true);
                return;
            }

            await NotifyAsync(Loc.Get("Msg_SyncFailed"), isError: true).ConfigureAwait(true);
        }

        private async Task<string> ResolveWorkingBranchAsync()
        {
            var cached = StripRebaseSuffix(CurrentBranch);
            if (!string.IsNullOrWhiteSpace(cached))
                return cached;

            var resolved = await TryResolveBranchNameAsync().ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                CurrentBranch = await IsRebaseInProgressAsync().ConfigureAwait(true)
                    ? resolved + " (rebase)"
                    : resolved;
                return resolved;
            }

            await NotifyAsync(Loc.Get("Msg_NoBranch"), isError: true).ConfigureAwait(true);
            return null;
        }

        private static string StripRebaseSuffix(string branch)
        {
            if (string.IsNullOrWhiteSpace(branch) || branch == UnknownBranch)
                return null;

            const string suffix = " (rebase)";
            var trimmed = branch.Trim();
            if (trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return trimmed.Substring(0, trimmed.Length - suffix.Length).Trim();

            return trimmed;
        }

        private async Task EnsureUpstreamAsync(string branch, string remoteBranch = null)
        {
            if (!HasAnyCommit)
                return;

            remoteBranch = string.IsNullOrWhiteSpace(remoteBranch) ? branch : remoteBranch.Trim();
            if (!await RemoteBranchExistsAsync(remoteBranch))
                return;

            var upstream = await RunGitQuietAsync("rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}");
            if (upstream.Success && !string.IsNullOrWhiteSpace(upstream.StandardOutput))
                return;

            await RunGitLoggedAsync("branch", "--set-upstream-to=origin/" + remoteBranch, branch);
        }

        private async Task<bool> TryAutoCommitAllAsync()
        {
            if (!await HasUncommittedChangesAsync())
                return true;

            AppendLog(Loc.Get("Msg_SyncAutoCommit"));
            await RunGitLoggedAsync("add", "-A");
            var message = ResolveAutoCommitMessage();
            var commit = await RunGitLoggedAsync("commit", "-m", message);
            if (!commit.Success)
            {
                var combined = commit.StandardOutput + commit.StandardError;
                if (combined.IndexOf("nothing to commit", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                return false;
            }

            CommitMessage = message;
            _settings.LastCommitMessage = message;
            PersistSettings();
            return true;
        }

        private string ResolveAutoCommitMessage()
        {
            if (!string.IsNullOrWhiteSpace(CommitMessage))
                return CommitMessage.Trim();

            if (!string.IsNullOrWhiteSpace(_settings.LastCommitMessage))
                return _settings.LastCommitMessage.Trim();

            return Loc.GetEnglish("Msg_AutoSyncCommit");
        }

        private async Task<bool> TryIntegrateRemoteAsync(string branch)
        {
            await RunGitQuietAsync("fetch", "origin");
            var counts = await GetAheadBehindAsync(branch);
            if (!counts.RemoteBranchExists || counts.Behind <= 0)
                return true;

            AppendLog(Loc.Get("Msg_SyncPulling"));
            var pull = await RunGitLoggedAsync("pull", "--rebase", "--autostash", "origin", branch);
            if (!pull.Success)
            {
                if (await TryAutoResolveKnownConflictsAsync() && await TryContinueRebaseAsync())
                    return !await HasMergeConflictsInTreeAsync();

                if (await IsRebaseInProgressAsync())
                {
                    await RunGitLoggedAsync("rebase", "--abort");
                    pull = await RunGitLoggedAsync("pull", "--autostash", "origin", branch);
                }

                if (!pull.Success)
                    pull = await RunGitLoggedAsync("pull", "origin", branch);
            }

            if (!pull.Success && await IsRebaseInProgressAsync())
                await RunGitLoggedAsync("rebase", "--abort");

            if (!pull.Success)
                return false;

            return !await HasMergeConflictsInTreeAsync();
        }

        private async Task<bool> TryRecoverInterruptedGitOperationAsync()
        {
            if (!await IsRebaseInProgressAsync())
                return true;

            AppendLog(Loc.Get("Msg_SyncRecoverRebase"));
            if (await TryAutoResolveKnownConflictsAsync())
                return await TryContinueRebaseAsync();

            await RunGitLoggedAsync("rebase", "--abort");
            return !await IsRebaseInProgressAsync();
        }

        private async Task<bool> IsRebaseInProgressAsync()
        {
            var gitDir = Path.Combine(RepoPath, ".git");
            if (Directory.Exists(Path.Combine(gitDir, "rebase-merge")) ||
                Directory.Exists(Path.Combine(gitDir, "rebase-apply")))
                return true;

            var status = await RunGitQuietAsync("status");
            if (!status.Success)
                return false;

            return status.StandardOutput.IndexOf("rebase in progress", StringComparison.OrdinalIgnoreCase) >= 0
                || status.StandardOutput.IndexOf("currently rebasing", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task<bool> TryAutoResolveKnownConflictsAsync()
        {
            var resolved = false;
            var status = await RunGitQuietAsync("status", "--porcelain");
            if (!status.Success || string.IsNullOrWhiteSpace(status.StandardOutput))
                return false;

            foreach (var line in status.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length < 4)
                    continue;

                var path = line.Substring(3).Trim();
                if (string.IsNullOrEmpty(path) || !IsAutoResolvableReleaseArtifact(path))
                    continue;

                var code = line.Substring(0, 2);
                if (code[0] != 'U' && code[1] != 'U' && code.IndexOf('D') < 0)
                    continue;

                var rm = await RunGitLoggedAsync("rm", "-f", "--", path);
                if (rm.Success)
                {
                    resolved = true;
                    AppendLog(string.Format(Loc.Get("Msg_SyncResolvedConflict"), path));
                }
            }

            return resolved;
        }

        private static bool IsAutoResolvableReleaseArtifact(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var name = Path.GetFileName(path);
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                name.StartsWith("WPFAutoGitHelper_v", StringComparison.OrdinalIgnoreCase))
                return true;

            return name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                && path.IndexOf(Path.DirectorySeparatorChar) < 0
                && path.IndexOf('/') < 0;
        }

        private async Task<bool> TryContinueRebaseAsync()
        {
            var result = await RunGitLoggedAsync("-c", "core.editor=true", "rebase", "--continue");
            if (result.Success)
                return true;

            if (await TryAutoResolveKnownConflictsAsync())
                result = await RunGitLoggedAsync("-c", "core.editor=true", "rebase", "--continue");

            return result.Success && !await HasMergeConflictsInTreeAsync();
        }

        private async Task<GitRunResult> TryPushWithRetryAsync(string branch)
        {
            var push = await RunGitLoggedAsync("push", "-u", "origin", branch);
            if (push.Success)
                return push;

            var err = push.StandardOutput + push.StandardError;
            if (!IsNonFastForwardPushError(err))
                return push;

            if (!await TryIntegrateRemoteAsync(branch))
                return push;

            if (await HasMergeConflictsInTreeAsync())
                return push;

            return await RunGitLoggedAsync("push", "-u", "origin", branch);
        }

        private async Task<AheadBehindCounts> GetAheadBehindAsync(string branch)
        {
            var counts = new AheadBehindCounts();
            var count = await RunGitQuietAsync("rev-list", "--left-right", "--count", "origin/" + branch + "...HEAD");
            if (!count.Success || string.IsNullOrWhiteSpace(count.StandardOutput))
                return counts;

            var parts = count.StandardOutput.Trim().Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return counts;

            if (!int.TryParse(parts[0], out var behind) || !int.TryParse(parts[1], out var ahead))
                return counts;

            counts.RemoteBranchExists = true;
            counts.Behind = behind;
            counts.Ahead = ahead;
            return counts;
        }

        private async Task<bool> HasUncommittedChangesAsync()
        {
            var status = await RunGitQuietAsync("status", "--porcelain");
            return status.Success && !string.IsNullOrWhiteSpace(status.StandardOutput);
        }

        private async Task<bool> HasMergeConflictsInTreeAsync()
        {
            var status = await RunGitQuietAsync("status", "--porcelain");
            if (!status.Success || string.IsNullOrWhiteSpace(status.StandardOutput))
                return false;

            foreach (var line in status.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length < 2)
                    continue;

                var x = line[0];
                var y = line.Length > 1 ? line[1] : ' ';
                if (x == 'U' || y == 'U' || (x == 'A' && y == 'A') || (x == 'D' && y == 'D'))
                    return true;
            }

            return false;
        }

        private static bool IsNonFastForwardPushError(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.IndexOf("non-fast-forward", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("failed to push some refs", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Updates were rejected", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task ClearWorkflowAsync()
        {
            if (!await ConfirmAsync(Loc.Get("Msg_ConfirmClearWorkflow"), Loc.Get("Dlg_ClearWorkflow")).ConfigureAwait(true))
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
                await NotifyAsync(Loc.Get("Msg_ReleaseGhRequired")).ConfigureAwait(true);
                return;
            }

            if (string.IsNullOrWhiteSpace(ReleaseTag))
            {
                await NotifyAsync(Loc.Get("Msg_ReleaseTagRequired")).ConfigureAwait(true);
                return;
            }

            var target = ReleaseTargetBranch;
            if (string.IsNullOrWhiteSpace(target) && CurrentBranch != UnknownBranch)
                target = CurrentBranch;

            if (ReleaseLatest && ReleasePrerelease)
            {
                await NotifyAsync(Loc.Get("Msg_ReleaseLatestPrereleaseConflict")).ConfigureAwait(true);
                return;
            }

            if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
                return;

            var request = new ReleaseRequest
            {
                Tag = ReleaseTag.Trim(),
                Title = ReleaseTitle?.Trim(),
                Notes = ReleaseNotes?.Trim(),
                TargetBranch = string.IsNullOrWhiteSpace(target) ? null : target.Trim(),
                IsLatest = ReleaseLatest,
                IsPrerelease = ReleasePrerelease,
                AssetPaths = ReleaseAssets.ToList(),
            };

            await RunWithBusyAsync(async () =>
            {
                var result = await GitHubCliRunner.CreateReleaseAsync(RepoPath, request, CancellationToken.None)
                    .ConfigureAwait(true);
                var assetLabel = request.AssetPaths.Count > 0
                    ? " +" + request.AssetPaths.Count + " file(s)"
                    : "";
                LogResult(result, "gh release create " + request.Tag + assetLabel);
                if (result.Success)
                {
                    PersistReleaseAssets();
                    await NotifyAsync(string.Format(Loc.Get("Msg_ReleaseCreated"), request.Tag)).ConfigureAwait(true);
                }
            }).ConfigureAwait(true);
        }

        private void AddReleaseAssets()
        {
            var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Title = Loc.Get("Dlg_ReleasePickFiles"),
                Filter = Loc.Get("Dlg_ReleaseFileFilter"),
                Multiselect = true,
            };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var added = 0;
            foreach (var file in dlg.FileNames)
            {
                if (AddReleaseAssetPath(file))
                    added++;
            }

            if (added > 0)
                PersistReleaseAssets();
        }

        private void AddReleaseImages()
        {
            var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Title = Loc.Get("Dlg_ReleasePickImages"),
                Filter = Loc.Get("Dlg_ReleaseImageFilter"),
                Multiselect = true,
            };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var added = 0;
            foreach (var file in dlg.FileNames)
            {
                if (AddReleaseAssetPath(file))
                    added++;
            }

            if (added > 0)
                PersistReleaseAssets();
        }

        private async Task AddReleaseBuildOutputAsync()
        {
            var added = 0;
            foreach (var path in GetDefaultReleaseBuildPaths())
            {
                if (AddReleaseAssetPath(path))
                    added++;
            }

            if (added == 0)
            {
                await NotifyAsync(Loc.Get("Msg_ReleaseBuildNotFound")).ConfigureAwait(true);
                return;
            }

            PersistReleaseAssets();
        }

        private IEnumerable<string> GetDefaultReleaseBuildPaths()
        {
            if (!HasValidRepo || string.IsNullOrWhiteSpace(RepoPath))
                yield break;

            var releaseDirs = new[]
            {
                Path.Combine(RepoPath, "WpfAutoGitHelper", "bin", "Release"),
                Path.Combine(RepoPath, "bin", "Release"),
            };

            var seenDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in releaseDirs)
            {
                if (!seenDirs.Add(dir) || !Directory.Exists(dir))
                    continue;

                var exe = Path.Combine(dir, "WpfAutoGitHelper.exe");
                if (File.Exists(exe))
                    yield return exe;

                var pdb = Path.Combine(dir, "WpfAutoGitHelper.pdb");
                if (File.Exists(pdb))
                    yield return pdb;
            }
        }

        private bool AddReleaseAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            path = Path.GetFullPath(path.Trim());
            foreach (var existing in ReleaseAssets)
            {
                if (string.Equals(existing, path, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            ReleaseAssets.Add(path);
            return true;
        }

        private void RemoveSelectedReleaseAsset()
        {
            if (string.IsNullOrWhiteSpace(SelectedReleaseAsset))
                return;

            for (var i = ReleaseAssets.Count - 1; i >= 0; i--)
            {
                if (string.Equals(ReleaseAssets[i], SelectedReleaseAsset, StringComparison.OrdinalIgnoreCase))
                    ReleaseAssets.RemoveAt(i);
            }

            SelectedReleaseAsset = null;
            PersistReleaseAssets();
        }

        private void PersistReleaseAssets()
        {
            _settings.LastReleaseAssetPaths = ReleaseAssets.ToList();
            PersistSettings();
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
                await NotifyAsync(Loc.Get("Msg_NoGitHub")).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_EnterNameEmail")).ConfigureAwait(true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(UserName))
                await RunGitLoggedAsync("config", "--global", "user.name", UserName.Trim());

            if (!string.IsNullOrWhiteSpace(UserEmail))
                await RunGitLoggedAsync("config", "--global", "user.email", UserEmail.Trim());
        }

        private async Task ClearGitIdentityAsync()
        {
            if (!await ConfirmAsync(Loc.Get("Msg_ConfirmClearIdentity"), Loc.Get("Dlg_ClearIdentity")).ConfigureAwait(true))
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
                await NotifyAsync(Loc.Get("Msg_EnterBranchName")).ConfigureAwait(true);
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
                await NotifyAsync(Loc.Get("Msg_SelectBranch")).ConfigureAwait(true);
                return;
            }

            if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("fetch", "origin");
            var push = await TryPushWithRetryAsync(branch);
            if (!push.Success)
                await ShowPublishFailureAsync();

            await RefreshStatusAsync();
        }

        private sealed class AheadBehindCounts
        {
            public bool RemoteBranchExists { get; set; }
            public int Behind { get; set; }
            public int Ahead { get; set; }
        }

        private async Task<string> ResolveCurrentBranchNameAsync(string repoPath, CancellationToken token)
        {
            var branch = await RunGitInDirectoryAsync(repoPath, token, "branch", "--show-current").ConfigureAwait(false);
            if (branch.Success && !string.IsNullOrWhiteSpace(branch.StandardOutput))
                return branch.StandardOutput.Trim();

            var headFile = await ReadGitHeadBranchFileInDirectoryAsync(repoPath, "rebase-merge/head-name").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(headFile))
                return headFile;

            headFile = await ReadGitHeadBranchFileInDirectoryAsync(repoPath, "rebase-apply/head-name").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(headFile))
                return headFile;

            return "main";
        }

        private static async Task<string> ReadGitHeadBranchFileInDirectoryAsync(string repoPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
                return null;

            var gitDirResult = await GitRunner.RunAsync(repoPath, CancellationToken.None, "rev-parse", "--git-dir").ConfigureAwait(false);
            if (!gitDirResult.Success || string.IsNullOrWhiteSpace(gitDirResult.StandardOutput))
                return null;

            var gitDir = gitDirResult.StandardOutput.Trim();
            if (!Path.IsPathRooted(gitDir))
                gitDir = Path.GetFullPath(Path.Combine(repoPath, gitDir));

            var filePath = Path.Combine(gitDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(filePath))
                return null;

            var text = (await Task.Run(() => File.ReadAllText(filePath)).ConfigureAwait(false)).Trim();
            return NormalizeRefsHeadsBranch(text);
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

            var manageBusy = _busyScopeDepth == 0;
            if (manageBusy)
                EnterBusyScope();

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
                    ExitBusyScope();
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

            if (isGlobalConfig)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var manageBusy = _busyScopeDepth == 0 && !_suppressGitCommandBusy;
                if (manageBusy)
                    EnterBusyScope();
                try
                {
                    var token = _operationCts?.Token ?? CancellationToken.None;
                    return await GitRunner.RunAsync(home, token, args).ConfigureAwait(true);
                }
                finally
                {
                    if (manageBusy)
                        ExitBusyScope();
                }
            }

            return await _gitOps.RunQuietAsync(args).ConfigureAwait(true);
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

        private static string ToSpellCheckLanguageTag(string code)
        {
            switch (Loc.Normalize(code))
            {
                case "ru": return "ru-RU";
                case "uk": return "uk-UA";
                case "de": return "de-DE";
                case "fr": return "fr-FR";
                case "es": return "es-ES";
                case "pt": return "pt-PT";
                case "pl": return "pl-PL";
                case "it": return "it-IT";
                case "nl": return "nl-NL";
                case "tr": return "tr-TR";
                case "zh": return "zh-CN";
                case "ja": return "ja-JP";
                case "ko": return "ko-KR";
                case "cs": return "cs-CZ";
                default: return "en-US";
            }
        }
    }
}

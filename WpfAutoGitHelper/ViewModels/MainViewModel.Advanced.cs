using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private readonly DangerousOperationGuard _dangerGuard = new DangerousOperationGuard();
        private string _mergeSourceBranch = "";
        private string _draftBranchName = "";
        private string _revertCommitHash = "";
        private string _selectedLocalBranchForDelete = "";
        private string _selectedRemoteBranchForDelete = "";
        private ChangedFileEntry _selectedChangedFile;
        private CommitLogEntry _selectedCommit;
        private ConflictFileEntry _selectedConflictFile;

        public ObservableCollection<ChangedFileEntry> ChangedFiles { get; } = new ObservableCollection<ChangedFileEntry>();
        public ObservableCollection<CommitLogEntry> RecentCommits { get; } = new ObservableCollection<CommitLogEntry>();
        public ObservableCollection<DangerousOperationRecord> SafetyLog { get; } = new ObservableCollection<DangerousOperationRecord>();
        public ObservableCollection<string> RemoteBranches { get; } = new ObservableCollection<string>();
        public ObservableCollection<ConflictFileEntry> ConflictFiles { get; } = new ObservableCollection<ConflictFileEntry>();

        public ChangedFileEntry SelectedChangedFile
        {
            get => _selectedChangedFile;
            set { _selectedChangedFile = value; OnPropertyChanged(); }
        }

        public CommitLogEntry SelectedCommit
        {
            get => _selectedCommit;
            set { _selectedCommit = value; OnPropertyChanged(); }
        }

        public string MergeSourceBranch
        {
            get => _mergeSourceBranch;
            set { _mergeSourceBranch = value ?? ""; OnPropertyChanged(); }
        }

        public string DraftBranchName
        {
            get => _draftBranchName;
            set
            {
                _draftBranchName = value ?? "";
                OnPropertyChanged();
                DraftBranchPushCommand?.RaiseCanExecuteChanged();
            }
        }

        public string RevertCommitHash
        {
            get => _revertCommitHash;
            set { _revertCommitHash = value ?? ""; OnPropertyChanged(); }
        }

        public string SelectedLocalBranchForDelete
        {
            get => _selectedLocalBranchForDelete;
            set { _selectedLocalBranchForDelete = value ?? ""; OnPropertyChanged(); }
        }

        public string SelectedRemoteBranchForDelete
        {
            get => _selectedRemoteBranchForDelete;
            set { _selectedRemoteBranchForDelete = value ?? ""; OnPropertyChanged(); }
        }

        public ConflictFileEntry SelectedConflictFile
        {
            get => _selectedConflictFile;
            set { _selectedConflictFile = value; OnPropertyChanged(); }
        }

        public ICommand RestoreSelectedFileCommand { get; private set; }
        public ICommand RefreshChangedFilesCommand { get; private set; }
        public ICommand StageSelectedFilesCommand { get; private set; }
        public ICommand UnstageSelectedFilesCommand { get; private set; }
        public ICommand CommitSelectedAndPushCommand { get; private set; }
        public ICommand AmendCommitCommand { get; private set; }
        public ICommand RevertCommitCommand { get; private set; }
        public ICommand ForcePushLeaseCommand { get; private set; }
        public ICommand ForcePushCommand { get; private set; }
        public RelayCommand DraftBranchPushCommand { get; private set; }
        public ICommand DeleteLocalBranchCommand { get; private set; }
        public ICommand DeleteRemoteBranchCommand { get; private set; }
        public ICommand PruneRemoteCommand { get; private set; }
        public ICommand MergeBranchCommand { get; private set; }
        public ICommand RebaseBranchCommand { get; private set; }
        public ICommand MergeContinueCommand { get; private set; }
        public ICommand MergeAbortCommand { get; private set; }
        public ICommand ResolveConflictsCommand { get; private set; }
        public ICommand GitRmSelectedFileCommand { get; private set; }
        public ICommand DeleteUntrackedFileCommand { get; private set; }
        public ICommand StagingSelectAllCommand { get; private set; }
        public ICommand StagingSelectNoneCommand { get; private set; }
        public ICommand RefreshCommitsCommand { get; private set; }
        public ICommand RefreshRemoteBranchesCommand { get; private set; }
        public ICommand RefreshAutoPreviewCommand { get; private set; }
        public ICommand RunAutoActionsCommand { get; private set; }

        private void InitAdvanced()
        {
            _dangerGuard.ConfirmYesNoAsync = async (title, body) => await ConfirmAsync(body, title).ConfigureAwait(true);
            _dangerGuard.RecordOperation = r =>
            {
                SafetyLog.Insert(0, r);
                if (SafetyLog.Count > 200)
                    SafetyLog.RemoveAt(SafetyLog.Count - 1);
            };

            RestoreSelectedFileCommand = new RelayCommand(async () => await RestoreSelectedFileAsync(), () => HasValidRepo && !IsBusy && SelectedChangedFile != null);
            RefreshChangedFilesCommand = new RelayCommand(async () => await RefreshChangedFilesAsync(), () => HasValidRepo && !IsBusy);
            StageSelectedFilesCommand = new RelayCommand(async () => await StageSelectedFilesAsync(true), () => HasValidRepo && !IsBusy);
            UnstageSelectedFilesCommand = new RelayCommand(async () => await StageSelectedFilesAsync(false), () => HasValidRepo && !IsBusy);
            CommitSelectedAndPushCommand = new RelayCommand(async () => await CommitSelectedAndPushAsync(), () => HasValidRepo && !IsBusy);
            AmendCommitCommand = new RelayCommand(async () => await AmendCommitAsync(), () => HasValidRepo && !IsBusy);
            RevertCommitCommand = new RelayCommand(async () => await RevertCommitAsync(), () => HasValidRepo && !IsBusy);
            ForcePushLeaseCommand = new RelayCommand(async () => await ForcePushAsync(useLease: true), () => HasValidRepo && !IsBusy);
            ForcePushCommand = new RelayCommand(async () => await ForcePushAsync(useLease: false), () => HasValidRepo && !IsBusy);
            DraftBranchPushCommand = new RelayCommand(
                async () => await DraftBranchPushAsync(),
                () => HasValidRepo && !IsBusy && !string.IsNullOrWhiteSpace(DraftBranchName));
            DeleteLocalBranchCommand = new RelayCommand(async () => await DeleteLocalBranchAsync(), () => HasValidRepo && !IsBusy);
            DeleteRemoteBranchCommand = new RelayCommand(async () => await DeleteRemoteBranchAsync(), () => HasValidRepo && !IsBusy);
            PruneRemoteCommand = new RelayCommand(async () => await PruneRemoteAsync(), () => HasValidRepo && !IsBusy);
            MergeBranchCommand = new RelayCommand(async () => await MergeBranchAsync(), () => HasValidRepo && !IsBusy);
            RebaseBranchCommand = new RelayCommand(async () => await RebaseBranchAsync(), () => HasValidRepo && !IsBusy);
            MergeContinueCommand = new RelayCommand(async () => await MergeContinueAsync(), () => HasValidRepo && !IsBusy);
            MergeAbortCommand = new RelayCommand(async () => await MergeAbortAsync(), () => HasValidRepo && !IsBusy);
            ResolveConflictsCommand = new RelayCommand(async () => await ResolveConflictsAsync(), () => HasValidRepo && !IsBusy);
            GitRmSelectedFileCommand = new RelayCommand(async () => await GitRmSelectedFileAsync(), () => HasValidRepo && !IsBusy && SelectedChangedFile != null);
            DeleteUntrackedFileCommand = new RelayCommand(async () => await DeleteUntrackedFileAsync(), () => HasValidRepo && !IsBusy && SelectedChangedFile != null);
            StagingSelectAllCommand = new RelayCommand(() => SetAllChangedFilesSelected(true), () => ChangedFiles.Count > 0);
            StagingSelectNoneCommand = new RelayCommand(() => SetAllChangedFilesSelected(false), () => ChangedFiles.Count > 0);
            RefreshCommitsCommand = new RelayCommand(async () => await RefreshRecentCommitsAsync(), () => HasValidRepo && !IsBusy);
            RefreshRemoteBranchesCommand = new RelayCommand(async () => await RefreshRemoteBranchesAsync(), () => HasValidRepo && !IsBusy);
            InitAutoAdvanced();

            var none = Loc.Get("Combo_None");
            SelectedLocalBranchForDelete = none;
            SelectedRemoteBranchForDelete = none;
            MergeSourceBranch = none;
            RebuildBranchPickerLists();
        }

        private async Task RefreshChangedFilesAsync()
        {
            if (!HasValidRepo)
                return;

            var status = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
            ChangedFiles.Clear();
            if (!status.Success)
                return;

            foreach (var entry in GitRunner.ParsePorcelain(status.StandardOutput))
            {
                entry.IsSelected = IsAutoAdvancedMode;
                ChangedFiles.Add(entry);
            }

            if (IsAutoAdvancedMode)
                AutoActionPreview = BuildAutoActionPreview(includeValidation: true, out _);
        }

        private async Task RestoreSelectedFileAsync()
        {
            var file = SelectedChangedFile;
            if (file == null)
                return;

            if (file.IsUntracked)
            {
                await NotifyAsync(Loc.Get("Msg_UntrackedRestore"), isError: true).ConfigureAwait(true);
                return;
            }

            if (_settings.ConfirmRestore &&
                !await ConfirmAsync(Loc.Get("Dlg_Restore"), Loc.Get("Btn_RestoreFile")).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("restore", "--", file.FilePath).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private void SetAllChangedFilesSelected(bool selected)
        {
            foreach (var f in ChangedFiles)
                f.IsSelected = selected;
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private bool _autoRunPull = true;
        private bool _autoUseSelectedFiles = true;
        private bool _autoRunCommit = true;
        private bool _autoRunPush = true;
        private bool _autoRunCreateRelease;
        private bool _autoRunCreateGithubRepo;
        private string _autoActionPreview = "";

        public bool AutoRunPull
        {
            get => _autoRunPull;
            set { if (_autoRunPull == value) return; _autoRunPull = value; OnPropertyChanged(); }
        }

        public bool AutoUseSelectedFiles
        {
            get => _autoUseSelectedFiles;
            set { if (_autoUseSelectedFiles == value) return; _autoUseSelectedFiles = value; OnPropertyChanged(); }
        }

        public bool AutoRunCommit
        {
            get => _autoRunCommit;
            set { if (_autoRunCommit == value) return; _autoRunCommit = value; OnPropertyChanged(); }
        }

        public bool AutoRunPush
        {
            get => _autoRunPush;
            set { if (_autoRunPush == value) return; _autoRunPush = value; OnPropertyChanged(); }
        }

        public bool AutoRunCreateRelease
        {
            get => _autoRunCreateRelease;
            set { if (_autoRunCreateRelease == value) return; _autoRunCreateRelease = value; OnPropertyChanged(); }
        }

        public bool AutoRunCreateGithubRepo
        {
            get => _autoRunCreateGithubRepo;
            set
            {
                if (_autoRunCreateGithubRepo == value)
                    return;
                _autoRunCreateGithubRepo = value;
                OnPropertyChanged();
                PersistSettings();
            }
        }

        public string AutoActionPreview
        {
            get => _autoActionPreview;
            private set
            {
                _autoActionPreview = value ?? "";
                OnPropertyChanged();
            }
        }

        private void InitAutoAdvanced()
        {
            RefreshAutoPreviewCommand = new RelayCommand(async () => await RefreshAutoActionPreviewAsync(), () => HasValidRepo && !IsBusy);
            RunAutoActionsCommand = new RelayCommand(async () => await RunAutoActionsAsync(), () => HasValidRepo && !IsBusy);
        }

        private async Task RefreshAutoActionPreviewAsync()
        {
            if (HasValidRepo)
            {
                await RefreshHasAnyCommitAsync().ConfigureAwait(true);
                await RefreshOriginRemoteUrlAsync().ConfigureAwait(true);
                await RefreshChangedFilesAsync().ConfigureAwait(true);
            }

            AutoActionPreview = BuildAutoActionPreview(includeValidation: true, out _);
        }

        private string BuildAutoActionPreview(bool includeValidation, out bool hasErrors)
        {
            var sb = new StringBuilder();
            hasErrors = false;
            var selectedCount = ChangedFiles.Count(f => f.IsSelected);
            var stageAllFallback = ChangedFiles.Count > 0 && selectedCount == 0;
            var initialCommitAddAll = !HasAnyCommit && stageAllFallback;

            sb.AppendLine(Loc.Get("Auto_PreviewContext"));
            sb.AppendLine($"{Loc.Get("Auto_SelectedProject")}: {SelectedProjectName}");
            sb.AppendLine($"{Loc.Get("Auto_SelectedFolder")}: {SelectedProjectFolder}");
            sb.AppendLine($"{Loc.Get("Repo_CurrentBranch")} {CurrentBranch}");
            sb.AppendLine($"{Loc.Get("Auto_HasCommits")}: {(HasAnyCommit ? Loc.Get("Auto_Yes") : Loc.Get("Auto_No"))}");
            sb.AppendLine($"{Loc.Get("Auto_SelectedFilesCount")}: {selectedCount}");
            if (AutoRunPush)
            {
                var originLine = string.IsNullOrWhiteSpace(OriginRemoteUrl)
                    ? Loc.Get("Auto_OriginUrlNotSet")
                    : OriginRemoteUrl.Trim();
                sb.AppendLine($"{Loc.Get("Label_OriginUrl")}: {originLine}");
            }

            sb.AppendLine();

            sb.AppendLine(Loc.Get("Auto_PlannedSteps"));
            if (AutoRunCreateGithubRepo && !HasValidRepo)
                sb.AppendLine($"- {Loc.Get("Auto_RunCreateGithubRepo")}");
            if (AutoRunPull)
                sb.AppendLine($"- {Loc.Get("Btn_Pull")}");
            if (AutoUseSelectedFiles)
            {
                if (stageAllFallback)
                    sb.AppendLine($"- {(initialCommitAddAll ? Loc.Get("Auto_StageAllInitialCommit") : Loc.Get("Auto_StageAllUnselected"))}");
                else
                    sb.AppendLine($"- {Loc.Get("Auto_StageSelectedFiles")}");
            }
            else
                sb.AppendLine($"- {Loc.Get("Btn_AddAll")}");
            if (AutoRunCommit)
                sb.AppendLine($"- {Loc.Get("Btn_Commit")}");
            if (AutoRunPush)
            {
                if (!string.IsNullOrWhiteSpace(DraftBranchName))
                    sb.AppendLine($"- {Loc.Get("Btn_DraftBranch")}: {DraftBranchName.Trim()}");
                else
                    sb.AppendLine($"- {Loc.Get("Btn_Push")}");
            }
            if (AutoRunCreateRelease)
                sb.AppendLine($"- {Loc.Get("Btn_CreateRelease")}: {ReleaseTag?.Trim()} ({Loc.Get("Auto_ReleaseTargetHint")})");

            if (!includeValidation)
                return sb.ToString().TrimEnd();

            var errors = new StringBuilder();
            if (!HasValidRepo && !AutoRunCreateGithubRepo)
                errors.AppendLine($"- {Loc.Get("Msg_NoValidRepo")}");
            if (AutoUseSelectedFiles && ChangedFiles.Count == 0 && (AutoRunCommit || AutoRunPush))
                errors.AppendLine($"- {Loc.Get("Auto_ErrNoFilesSelected")}");
            if (AutoRunCommit && string.IsNullOrWhiteSpace(CommitMessage))
                errors.AppendLine($"- {Loc.Get("Msg_EnterCommit")}");
            if (AutoRunCreateRelease && string.IsNullOrWhiteSpace(ReleaseTag))
                errors.AppendLine($"- {Loc.Get("Msg_ReleaseTagRequired")}");
            if (AutoRunPush && HasValidRepo && string.IsNullOrWhiteSpace(OriginRemoteUrl))
                errors.AppendLine($"- {Loc.Get("Auto_ErrOriginUrlEmpty")}");
            if (!AutoRunCreateGithubRepo && !AutoRunPull && !AutoRunCommit && !AutoRunPush && !AutoRunCreateRelease)
                errors.AppendLine($"- {Loc.Get("Auto_ErrNoActionsSelected")}");

            sb.AppendLine();
            if (errors.Length == 0)
            {
                sb.AppendLine(Loc.Get("Auto_NoErrors"));
            }
            else
            {
                hasErrors = true;
                sb.AppendLine(Loc.Get("Auto_ErrorsTitle"));
                sb.Append(errors.ToString().TrimEnd());
            }

            return sb.ToString().TrimEnd();
        }

        private Task RunAutoActionsAsync() => RunWithBusyAsync(RunAutoActionsCoreAsync);

        private async Task RunAutoActionsCoreAsync()
        {
            if (HasValidRepo)
                await RefreshStatusAsync().ConfigureAwait(true);

            AutoActionPreview = BuildAutoActionPreview(includeValidation: true, out var hasErrors);
            if (hasErrors)
            {
                await NotifyAsync(Loc.Get("Auto_ReviewErrorsFirst"), isError: true).ConfigureAwait(true);
                return;
            }

            AppendLog(Loc.Get("Auto_RunStarted"));

            if (HasValidRepo && !await TryAdvanceRebaseIfCleanAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Auto_RunStopped"), true);
                return;
            }

            if (HasValidRepo)
                await RefreshStatusAsync().ConfigureAwait(true);

            if (AutoRunCreateGithubRepo && !HasValidRepo)
            {
                await CreateNewRepoAsync().ConfigureAwait(true);
                if (!HasValidRepo)
                {
                    AppendLog(Loc.Get("Msg_NoValidRepo"), true);
                    return;
                }

                await RefreshStatusAsync().ConfigureAwait(true);
            }

            if (AutoRunPull && !await PullAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Auto_RunStopped"), true);
                return;
            }

            if (await HasWorkingTreeChangesAsync().ConfigureAwait(true))
            {
                if (AutoUseSelectedFiles)
                {
                    if (!await StageSelectedFilesAsync(true).ConfigureAwait(true))
                    {
                        AppendLog(Loc.Get("Auto_RunStopped"), true);
                        return;
                    }
                }
                else
                {
                    await AddAllAsync().ConfigureAwait(true);
                }
            }
            else
            {
                AppendLog(Loc.Get("Auto_SkipStageClean"));
            }

            if (AutoRunCommit)
            {
                if (!await HasWorkingTreeChangesAsync().ConfigureAwait(true))
                    AppendLog(Loc.Get("Auto_SkipCommitClean"));
                else if (!await CommitAsync().ConfigureAwait(true))
                {
                    AppendLog(Loc.Get("Auto_RunStopped"), true);
                    return;
                }
            }

            if (AutoRunPush)
            {
                var pushOk = !string.IsNullOrWhiteSpace(DraftBranchName)
                    ? await DraftBranchPushAsync().ConfigureAwait(true)
                    : await TryAutoPushAsync().ConfigureAwait(true);
                if (!pushOk)
                {
                    AppendLog(Loc.Get("Auto_RunStopped"), true);
                    return;
                }
            }

            if (AutoRunCreateRelease && !await CreateReleaseAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Auto_RunStopped"), true);
                return;
            }

            await RefreshStatusAsync().ConfigureAwait(true);
            AutoActionPreview = BuildAutoActionPreview(includeValidation: true, out _);
            AppendLog(Loc.Get("Auto_RunFinished"));
        }
    }
}

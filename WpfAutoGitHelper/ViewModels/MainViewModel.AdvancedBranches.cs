using System;
using System.IO;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Views;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task RefreshRemoteBranchesAsync()
        {
            await RunGitLoggedAsync("fetch", "--prune", "origin").ConfigureAwait(true);
            RemoteBranches.Clear();
            var result = await RunGitQuietAsync("branch", "-r").ConfigureAwait(true);
            if (!result.Success)
                return;

            foreach (var line in result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var b = line.Trim();
                if (b.StartsWith("origin/HEAD", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (b.StartsWith("origin/", StringComparison.OrdinalIgnoreCase))
                    RemoteBranches.Add(b.Substring("origin/".Length));
            }

            RebuildBranchPickerLists();
        }

        private async Task DeleteLocalBranchAsync()
        {
            if (IsBranchPickerNone(SelectedLocalBranchForDelete))
            {
                await NotifyAsync(Loc.Get("Msg_SelectBranchToDelete"), isError: true).ConfigureAwait(true);
                return;
            }

            var branch = SelectedLocalBranchForDelete.Trim();
            if (string.IsNullOrWhiteSpace(branch))
                return;

            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.DeleteLocalBranch, Loc.Get("Btn_DeleteLocalBranch"), branch, null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("branch", "-D", branch.Trim()).ConfigureAwait(true);
            await NotifyAsync(string.Format(Loc.Get("Msg_BranchDeleted"), branch)).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task DeleteRemoteBranchAsync()
        {
            if (IsBranchPickerNone(SelectedRemoteBranchForDelete))
            {
                await NotifyAsync(Loc.Get("Msg_SelectBranchToDelete"), isError: true).ConfigureAwait(true);
                return;
            }

            var branch = SelectedRemoteBranchForDelete.Trim();
            if (string.IsNullOrWhiteSpace(branch))
                return;

            if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
                return;

            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.DeleteRemoteBranch, Loc.Get("Btn_DeleteRemoteBranch"), branch, null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("push", "origin", "--delete", branch.Trim()).ConfigureAwait(true);
            await NotifyAsync(string.Format(Loc.Get("Msg_BranchDeleted"), branch)).ConfigureAwait(true);
            await RefreshRemoteBranchesAsync().ConfigureAwait(true);
        }

        private async Task PruneRemoteAsync()
        {
            await RunGitLoggedAsync("fetch", "--prune", "origin").ConfigureAwait(true);
            await RefreshRemoteBranchesAsync().ConfigureAwait(true);
            await RefreshBranchesAsync().ConfigureAwait(true);
        }

        private async Task MergeBranchAsync()
        {
            if (IsBranchPickerNone(MergeSourceBranch))
            {
                await NotifyAsync(Loc.Get("Msg_EnterBranchName"), isError: true).ConfigureAwait(true);
                return;
            }

            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.Merge, Loc.Get("Btn_MergeBranch"), MergeSourceBranch, null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("merge", MergeSourceBranch.Trim()).ConfigureAwait(true);
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task RebaseBranchAsync()
        {
            if (IsBranchPickerNone(MergeSourceBranch))
            {
                await NotifyAsync(Loc.Get("Msg_EnterBranchName"), isError: true).ConfigureAwait(true);
                return;
            }

            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.Rebase, Loc.Get("Btn_RebaseBranch"), MergeSourceBranch, null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("rebase", MergeSourceBranch.Trim()).ConfigureAwait(true);
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task MergeContinueAsync()
        {
            var merge = await RunGitLoggedAsync("-c", "core.editor=true", "merge", "--continue").ConfigureAwait(true);
            if (!merge.Success)
                await RunGitLoggedAsync("-c", "core.editor=true", "rebase", "--continue").ConfigureAwait(true);

            await NotifyAsync(Loc.Get("Msg_ConflictContinue")).ConfigureAwait(true);
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task MergeAbortAsync()
        {
            await RunGitLoggedAsync("merge", "--abort").ConfigureAwait(true);
            await RunGitLoggedAsync("rebase", "--abort").ConfigureAwait(true);
            await NotifyAsync(Loc.Get("Msg_ConflictAbort")).ConfigureAwait(true);
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task RefreshConflictFilesAsync()
        {
            ConflictFiles.Clear();
            var status = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
            if (!status.Success)
                return;

            foreach (var line in status.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length < 2)
                    continue;
                var x = line[0];
                var y = line[1];
                if (x != 'U' && y != 'U' && !(x == 'A' && y == 'A') && !(x == 'D' && y == 'D'))
                    continue;

                ConflictFiles.Add(new ConflictFileEntry
                {
                    StatusCode = line.Substring(0, 2),
                    FilePath = line.Length > 3 ? line.Substring(3).Trim() : "",
                });
            }
        }

        private async Task ResolveConflictsAsync()
        {
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            var path = SelectedConflictFile?.FilePath;
            if (string.IsNullOrWhiteSpace(path) && ConflictFiles.Count > 0)
                path = ConflictFiles[0].FilePath;

            if (string.IsNullOrWhiteSpace(path))
            {
                await NotifyAsync(Loc.Get("Msg_NoConflicts"), isError: true).ConfigureAwait(true);
                return;
            }

            var fullPath = Path.Combine(RepoPath, path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                await NotifyAsync(Loc.Get("Msg_FileNotFound"), isError: true).ConfigureAwait(true);
                return;
            }

            var window = new ConflictResolutionWindow(RepoPath, path, fullPath);
            if (window.ShowDialog() != true)
                return;

            await RunGitLoggedAsync("add", "--", path).ConfigureAwait(true);
            await RefreshConflictFilesAsync().ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }
    }
}

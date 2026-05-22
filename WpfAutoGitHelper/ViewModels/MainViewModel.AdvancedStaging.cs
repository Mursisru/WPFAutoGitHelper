using System.Linq;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task<bool> StageSelectedFilesAsync(bool stage)
        {
            var paths = ChangedFiles.Where(f => f.IsSelected).Select(f => f.FilePath).ToList();
            if (paths.Count == 0)
            {
                if (stage && ChangedFiles.Count > 0)
                {
                    AppendLog(HasAnyCommit
                        ? Loc.Get("Auto_StageAllUnselected")
                        : Loc.Get("Auto_StageAllInitialCommit"));
                    await RunGitLoggedAsync("add", "-A").ConfigureAwait(true);
                    await RefreshStatusAsync().ConfigureAwait(true);
                    return true;
                }

                await NotifyAsync(Loc.Get("Msg_SelectFiles"), isError: true).ConfigureAwait(true);
                return false;
            }

            if (stage)
            {
                foreach (var path in paths)
                    await RunGitLoggedAsync("add", "--", path).ConfigureAwait(true);
            }
            else
            {
                foreach (var path in paths)
                    await RunGitLoggedAsync("restore", "--staged", "--", path).ConfigureAwait(true);
            }

            await RefreshStatusAsync().ConfigureAwait(true);
            return true;
        }

        private async Task CommitSelectedAndPushAsync()
        {
            await StageSelectedFilesAsync(true).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(CommitMessage))
            {
                await NotifyAsync(Loc.Get("Msg_EnterCommit")).ConfigureAwait(true);
                return;
            }

            await CommitAsync().ConfigureAwait(true);
            await PushAsync().ConfigureAwait(true);
        }

        private async Task AmendCommitAsync()
        {
            if (!await _dangerGuard.ConfirmAsync(
                    DangerousOperationType.Amend,
                    Loc.Get("Btn_AmendCommit"),
                    Loc.Get("Msg_ConfirmAmend"),
                    Loc.Get("Msg_AmendUndoHint")).ConfigureAwait(true))
                return;

            var args = string.IsNullOrWhiteSpace(CommitMessage)
                ? new[] { "commit", "--amend", "--no-edit" }
                : new[] { "commit", "--amend", "-m", CommitMessage };

            await RunGitLoggedAsync(args).ConfigureAwait(true);
            await NotifyAsync(Loc.Get("Msg_AmendDone")).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task RevertCommitAsync()
        {
            string hash = null;
            if (!string.IsNullOrWhiteSpace(RevertCommitHash))
                hash = RevertCommitHash.Trim();
            else if (!string.IsNullOrWhiteSpace(SelectedCommit?.Hash))
                hash = SelectedCommit.Hash;

            if (string.IsNullOrWhiteSpace(hash))
            {
                await NotifyAsync(Loc.Get("Msg_SelectCommit"), isError: true).ConfigureAwait(true);
                return;
            }

            if (!await _dangerGuard.ConfirmAsync(
                    DangerousOperationType.Revert,
                    Loc.Get("Btn_RevertCommit"),
                    hash,
                    null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("revert", "--no-edit", hash).ConfigureAwait(true);
            await NotifyAsync(Loc.Get("Msg_RevertDone")).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task RefreshRecentCommitsAsync()
        {
            RecentCommits.Clear();
            RecentCommits.Add(new CommitLogEntry { Hash = "", ShortHash = "", Subject = Loc.Get("Combo_None") });
            var log = await RunGitQuietAsync("log", "-20", "--format=%H|%h|%s").ConfigureAwait(true);
            if (!log.Success)
                return;

            foreach (var line in log.StandardOutput.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('|');
                if (parts.Length < 3)
                    continue;
                RecentCommits.Add(new CommitLogEntry
                {
                    Hash = parts[0],
                    ShortHash = parts[1],
                    Subject = parts[2],
                });
            }

            SelectedCommit = RecentCommits.Count > 0 ? RecentCommits[0] : null;
        }
    }
}

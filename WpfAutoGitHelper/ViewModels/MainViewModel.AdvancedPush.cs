using System;
using System.Linq;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task ForcePushAsync(bool useLease)
        {
            var branch = await ResolveWorkingBranchAsync().ConfigureAwait(true);
            if (branch == null)
                return;

            if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
                return;

            var ahead = await RunGitQuietAsync("log", $"origin/{branch}..HEAD", "--oneline").ConfigureAwait(true);
            var behind = await RunGitQuietAsync("log", $"HEAD..origin/{branch}", "--oneline").ConfigureAwait(true);
            var preview = string.Format(
                Loc.Get("Msg_ForcePushPreview"),
                string.IsNullOrWhiteSpace(ahead.StandardOutput) ? "(none)" : ahead.StandardOutput.Trim(),
                string.IsNullOrWhiteSpace(behind.StandardOutput) ? "(none)" : behind.StandardOutput.Trim());

            var type = useLease ? DangerousOperationType.ForcePushLease : DangerousOperationType.ForcePush;
            if (!await _dangerGuard.ConfirmAsync(type, Loc.Get("Msg_ConfirmForcePush"), preview, null).ConfigureAwait(true))
                return;

            if (!useLease &&
                !await _dangerGuard.ConfirmAsync(DangerousOperationType.ForcePush, Loc.Get("Btn_ForcePush"), Loc.Get("Msg_ConfirmPlainForce"), null).ConfigureAwait(true))
                return;

            var pushArgs = useLease
                ? new[] { "push", "--force-with-lease", "origin", branch }
                : new[] { "push", "--force", "origin", branch };

            await RunGitLoggedAsync(pushArgs).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task<bool> DraftBranchPushAsync()
        {
            if (string.IsNullOrWhiteSpace(DraftBranchName))
            {
                await NotifyAsync(Loc.Get("Msg_DraftBranchNameRequired"), isError: true).ConfigureAwait(true);
                return false;
            }

            if (!HasAnyCommit)
            {
                await NotifyAsync(Loc.Get("Msg_NeedCommitBeforePush"), isError: true).ConfigureAwait(true);
                return false;
            }

            var name = DraftBranchName.Trim();

            if (IsAutoAdvancedMode)
            {
                if (!await EnsureOriginForAutoAsync().ConfigureAwait(true))
                    return false;
            }
            else if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
            {
                return false;
            }

            AppendLog(string.Format(Loc.Get("Msg_DraftBranchPushToGithub"), name));
            var push = await RunGitLoggedAsync("push", "-u", "origin", "HEAD:refs/heads/" + name).ConfigureAwait(true);
            if (!push.Success)
            {
                await NotifyAsync(Loc.Get("Msg_DraftBranchFailed"), isError: true).ConfigureAwait(true);
                await RefreshStatusAsync().ConfigureAwait(true);
                return false;
            }

            await RunGitLoggedAsync("fetch", "origin", name).ConfigureAwait(true);
            await NotifyAsync(string.Format(Loc.Get("Msg_DraftBranchCreated"), name)).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
            return true;
        }
    }
}

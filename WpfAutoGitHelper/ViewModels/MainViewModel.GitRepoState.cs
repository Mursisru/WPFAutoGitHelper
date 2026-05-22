using System;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private bool _hasAnyCommit = true;

        public bool HasAnyCommit
        {
            get => _hasAnyCommit;
            private set
            {
                if (_hasAnyCommit == value)
                    return;
                _hasAnyCommit = value;
                OnPropertyChanged();
            }
        }

        private async Task RefreshHasAnyCommitAsync()
        {
            if (!HasValidRepo)
            {
                HasAnyCommit = true;
                return;
            }

            var head = await RunGitQuietAsync("rev-parse", "--verify", "HEAD").ConfigureAwait(true);
            HasAnyCommit = head.Success;
        }

        private Task<bool> HasAnyCommitAsync() => Task.FromResult(HasAnyCommit);

        private async Task<string> GetOriginDefaultBranchAsync()
        {
            var sym = await RunGitQuietAsync("symbolic-ref", "refs/remotes/origin/HEAD").ConfigureAwait(true);
            if (sym.Success && !string.IsNullOrWhiteSpace(sym.StandardOutput))
            {
                const string prefix = "refs/remotes/origin/";
                var value = sym.StandardOutput.Trim();
                if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return value.Substring(prefix.Length);
            }

            foreach (var candidate in new[] { "main", "master" })
            {
                var verify = await RunGitQuietAsync("rev-parse", "--verify", "origin/" + candidate).ConfigureAwait(true);
                if (verify.Success)
                    return candidate;
            }

            return null;
        }

        private async Task<bool> RemoteBranchExistsAsync(string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return false;
            var verify = await RunGitQuietAsync("rev-parse", "--verify", "origin/" + branch.Trim()).ConfigureAwait(true);
            return verify.Success;
        }

        private async Task<bool> TryAutoPushAsync()
        {
            var branch = await ResolveWorkingBranchAsync();
            if (branch == null)
                return false;

            if (!HasAnyCommit)
            {
                await NotifyAsync(Loc.Get("Msg_NeedCommitBeforePush"), isError: true).ConfigureAwait(true);
                return false;
            }

            if (IsAutoAdvancedMode)
            {
                if (!await EnsureOriginForAutoAsync().ConfigureAwait(true))
                    return false;
            }
            else if (!await EnsureOriginRemoteAsync(forcePrompt: true).ConfigureAwait(true))
            {
                return false;
            }

            await RunGitLoggedAsync("fetch", "origin");

            var push = await RunGitLoggedAsync("push", "-u", "origin", branch);
            if (push.Success)
                return true;

            var remoteDefault = await GetOriginDefaultBranchAsync();
            if (!string.IsNullOrWhiteSpace(remoteDefault) &&
                !string.Equals(branch, remoteDefault, StringComparison.OrdinalIgnoreCase))
            {
                push = await RunGitLoggedAsync("push", "-u", "origin", "HEAD:" + remoteDefault);
                if (push.Success)
                {
                    AppendLog(string.Format(Loc.Get("Msg_PushedToRemoteBranch"), remoteDefault));
                    return true;
                }
            }

            await NotifyAsync(Loc.Get("Msg_PushFailedHint"), isError: true).ConfigureAwait(true);
            return false;
        }
    }
}

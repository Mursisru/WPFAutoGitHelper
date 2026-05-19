using System;
using System.IO;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task<string> TryResolveBranchNameAsync()
        {
            var branch = await RunGitQuietAsync("branch", "--show-current").ConfigureAwait(true);
            if (branch.Success && !string.IsNullOrWhiteSpace(branch.StandardOutput))
                return branch.StandardOutput.Trim();

            var fromRebase = await ReadGitHeadBranchFileAsync("rebase-merge/head-name").ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(fromRebase))
                return fromRebase;

            fromRebase = await ReadGitHeadBranchFileAsync("rebase-apply/head-name").ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(fromRebase))
                return fromRebase;

            var status = await RunGitQuietAsync("status").ConfigureAwait(true);
            if (status.Success)
            {
                var parsed = ParseRebasingBranchFromStatus(status.StandardOutput);
                if (!string.IsNullOrWhiteSpace(parsed))
                    return parsed;
            }

            return null;
        }

        private async Task<string> ReadGitHeadBranchFileAsync(string relativePath)
        {
            if (!HasValidRepo || string.IsNullOrWhiteSpace(RepoPath))
                return null;

            var gitDirResult = await RunGitQuietAsync("rev-parse", "--git-dir").ConfigureAwait(true);
            if (!gitDirResult.Success || string.IsNullOrWhiteSpace(gitDirResult.StandardOutput))
                return null;

            var gitDir = gitDirResult.StandardOutput.Trim();
            if (!Path.IsPathRooted(gitDir))
                gitDir = Path.GetFullPath(Path.Combine(RepoPath, gitDir));

            var filePath = Path.Combine(gitDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(filePath))
                return null;

            var text = (await Task.Run(() => File.ReadAllText(filePath)).ConfigureAwait(true)).Trim();
            return NormalizeRefsHeadsBranch(text);
        }

        private static string NormalizeRefsHeadsBranch(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            const string prefix = "refs/heads/";
            return raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? raw.Substring(prefix.Length)
                : raw;
        }

        private static string ParseRebasingBranchFromStatus(string statusOutput)
        {
            if (string.IsNullOrWhiteSpace(statusOutput))
                return null;

            const string marker = "rebasing branch '";
            var idx = statusOutput.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return null;

            idx += marker.Length;
            var end = statusOutput.IndexOf('\'', idx);
            return end > idx ? statusOutput.Substring(idx, end - idx) : null;
        }

        private async Task<bool> TryAdvanceRebaseIfCleanAsync()
        {
            if (!await IsRebaseInProgressAsync().ConfigureAwait(true))
                return true;

            AppendLog(Loc.Get("Msg_RebaseAutoRecover"));
            const int maxSteps = 32;
            for (var step = 0; step < maxSteps; step++)
            {
                if (!await IsRebaseInProgressAsync().ConfigureAwait(true))
                    return true;

                if (await HasMergeConflictsInTreeAsync().ConfigureAwait(true))
                {
                    if (await TryAutoResolveKnownConflictsAsync().ConfigureAwait(true))
                    {
                        if (!await HasMergeConflictsInTreeAsync().ConfigureAwait(true) &&
                            await TryContinueRebaseAsync().ConfigureAwait(true))
                            continue;
                    }

                    if (await HasMergeConflictsInTreeAsync().ConfigureAwait(true) &&
                        await TryAutoRebaseSkipCurrentPickAsync().ConfigureAwait(true))
                        continue;

                    AppendLog(Loc.Get("Msg_RebaseConflicts"), true);
                    return false;
                }

                if (await HasWorkingTreeChangesAsync().ConfigureAwait(true))
                {
                    if (!await TryStageAndCommitRebaseChangesAsync().ConfigureAwait(true))
                    {
                        AppendLog(Loc.Get("Msg_RebaseInProgressDirty"), true);
                        return false;
                    }
                }

                if (!await TryContinueRebaseAsync().ConfigureAwait(true))
                {
                    if (await HasMergeConflictsInTreeAsync().ConfigureAwait(true))
                    {
                        if (await TryAutoResolveKnownConflictsAsync().ConfigureAwait(true) &&
                            await TryContinueRebaseAsync().ConfigureAwait(true))
                            continue;

                        if (await TryAutoRebaseSkipCurrentPickAsync().ConfigureAwait(true))
                            continue;
                    }

                    if (await IsRebaseInProgressAsync().ConfigureAwait(true))
                        return false;
                }
            }

            if (await IsRebaseInProgressAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Msg_RebaseStillInProgress"), true);
                return false;
            }

            AppendLog(Loc.Get("Msg_RebaseFinished"));
            return true;
        }

        private async Task<bool> TryStageAndCommitRebaseChangesAsync()
        {
            AppendLog(Loc.Get("Msg_RebaseAutoStage"));
            var add = await RunGitLoggedAsync("add", "-A").ConfigureAwait(true);
            if (!add.Success)
                return false;

            var status = await RunGitQuietAsync("status").ConfigureAwait(true);
            var text = (status.StandardOutput ?? "") + (status.StandardError ?? "");
            var editing = text.IndexOf("currently editing a commit", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("You are currently editing", StringComparison.OrdinalIgnoreCase) >= 0;

            if (editing)
            {
                AppendLog(Loc.Get("Msg_RebaseAutoAmend"));
                var amend = await RunGitLoggedAsync("commit", "--amend", "--no-edit").ConfigureAwait(true);
                return amend.Success;
            }

            var porcelain = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
            if (!porcelain.Success || string.IsNullOrWhiteSpace(porcelain.StandardOutput))
                return true;

            var message = string.IsNullOrWhiteSpace(CommitMessage)
                ? Loc.Get("Msg_RebaseAutoCommitMessage")
                : CommitMessage.Trim();
            AppendLog(Loc.Get("Msg_RebaseAutoCommit"));
            var commit = await RunGitLoggedAsync("commit", "-m", message).ConfigureAwait(true);
            return commit.Success;
        }
    }
}

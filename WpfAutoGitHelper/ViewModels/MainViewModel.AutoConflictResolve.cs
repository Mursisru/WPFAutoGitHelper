using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task<IReadOnlyList<string>> GetUnmergedPathsAsync()
        {
            var paths = new List<string>();
            var diff = await RunGitQuietAsync("diff", "--name-only", "--diff-filter=U").ConfigureAwait(true);
            if (diff.Success && !string.IsNullOrWhiteSpace(diff.StandardOutput))
            {
                foreach (var line in diff.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var p = line.Trim();
                    if (!string.IsNullOrEmpty(p))
                        paths.Add(p);
                }
            }

            var status = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
            if (status.Success && !string.IsNullOrWhiteSpace(status.StandardOutput))
            {
                foreach (var line in status.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.Length < 4)
                        continue;

                    var code = line.Substring(0, 2);
                    if (!IsUnmergedPorcelainCode(code))
                        continue;

                    var p = line.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(p) && !paths.Any(x => string.Equals(x, p, StringComparison.OrdinalIgnoreCase)))
                        paths.Add(p);
                }
            }

            return paths;
        }

        private static bool IsUnmergedPorcelainCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 2)
                return false;

            if (code[0] == 'U' || code[1] == 'U')
                return true;

            return code == "AA" || code == "DD" || code == "AU" || code == "UA" || code == "DU" || code == "UD";
        }

        private async Task<bool> TryAutoResolveKnownConflictsAsync()
        {
            if (IsAutoAdvancedMode)
                return await TryAutoResolveAllConflictsForAutoAsync().ConfigureAwait(true);

            return await TryAutoResolveReleaseZipConflictsAsync().ConfigureAwait(true);
        }

        private async Task<bool> TryAutoResolveReleaseZipConflictsAsync()
        {
            var resolved = false;
            var status = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
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

                var rm = await RunGitLoggedAsync("rm", "-f", "--", path).ConfigureAwait(true);
                if (rm.Success)
                {
                    resolved = true;
                    AppendLog(string.Format(Loc.Get("Msg_SyncResolvedConflict"), path));
                }
            }

            return resolved;
        }

        private async Task<bool> TryAutoResolveAllConflictsForAutoAsync()
        {
            var paths = await GetUnmergedPathsAsync().ConfigureAwait(true);
            if (paths.Count == 0)
                return false;

            AppendLog(string.Format(Loc.Get("Msg_AutoConflictResolveStart"), paths.Count));
            var resolved = false;
            foreach (var path in paths)
            {
                if (await TryAutoResolveConflictPathForAutoAsync(path).ConfigureAwait(true))
                    resolved = true;
            }

            if (resolved)
                await RunGitLoggedAsync("add", "-A").ConfigureAwait(true);

            if (await HasMergeConflictsInTreeAsync().ConfigureAwait(true))
            {
                AppendLog(Loc.Get("Msg_AutoConflictCheckoutOurs"));
                foreach (var path in await GetUnmergedPathsAsync().ConfigureAwait(true))
                {
                    if (File.Exists(Path.Combine(RepoPath, path)))
                        await RunGitLoggedAsync("checkout", "--ours", "--", path).ConfigureAwait(true);
                    await RunGitLoggedAsync("add", "--", path).ConfigureAwait(true);
                }

                await RunGitLoggedAsync("add", "-A").ConfigureAwait(true);
                resolved = true;
            }

            return resolved && !await HasMergeConflictsInTreeAsync().ConfigureAwait(true);
        }

        private async Task<bool> TryAutoResolveConflictPathForAutoAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var fullPath = Path.Combine(RepoPath, path);
            if (File.Exists(fullPath))
            {
                var text = await Task.Run(() => File.ReadAllText(fullPath)).ConfigureAwait(true);
                if (text.IndexOf("<<<<<<<", StringComparison.Ordinal) >= 0)
                {
                    var hunks = ConflictMarkerParser.Parse(text);
                    var choice = GetAutoConflictResolutionChoice(path);
                    var resolvedText = ConflictMarkerParser.ApplyResolution(text, hunks, choice);
                    await Task.Run(() => File.WriteAllText(fullPath, resolvedText)).ConfigureAwait(true);
                    AppendLog(string.Format(Loc.Get("Msg_AutoConflictResolved"), path, choice));
                }
                else
                {
                    AppendLog(string.Format(Loc.Get("Msg_AutoConflictKeepFile"), path));
                }

                var add = await RunGitLoggedAsync("add", "--", path).ConfigureAwait(true);
                return add.Success;
            }

            var rm = await RunGitLoggedAsync("rm", "-f", "--", path).ConfigureAwait(true);
            if (rm.Success)
                AppendLog(string.Format(Loc.Get("Msg_AutoConflictAcceptDelete"), path));
            return rm.Success;
        }

        private static ConflictResolutionChoice GetAutoConflictResolutionChoice(string path)
        {
            var name = Path.GetFileName(path);
            if (name.Equals("CHANGELOG.md", StringComparison.OrdinalIgnoreCase)
                || name.Equals("PRE-RELEASE.md", StringComparison.OrdinalIgnoreCase)
                || name.Equals("VERSION.txt", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                return ConflictResolutionChoice.Ours;

            return ConflictResolutionChoice.Ours;
        }

        private async Task<bool> TryAutoRebaseSkipCurrentPickAsync()
        {
            if (!IsAutoAdvancedMode)
                return false;

            var message = await ReadRebaseCurrentCommitMessageAsync().ConfigureAwait(true);
            AppendLog(string.Format(Loc.Get("Msg_RebaseAutoSkip"), string.IsNullOrWhiteSpace(message) ? "?" : message.Trim()));
            var skip = await RunGitLoggedAsync("rebase", "--skip").ConfigureAwait(true);
            return skip.Success;
        }

        private async Task<string> ReadRebaseCurrentCommitMessageAsync()
        {
            var fromFile = await ReadGitHeadBranchFileAsync("rebase-merge/message").ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(fromFile))
                return fromFile;

            var show = await RunGitQuietAsync("show", "-s", "--format=%s", "REBASE_HEAD").ConfigureAwait(true);
            return show.Success ? show.StandardOutput?.Trim() : null;
        }
    }
}

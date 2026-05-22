using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task<string> GetUpstreamRemoteBranchNameAsync()
        {
            var upstream = await RunGitQuietAsync("rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}")
                .ConfigureAwait(true);
            if (!upstream.Success || string.IsNullOrWhiteSpace(upstream.StandardOutput))
                return null;

            const string prefix = "origin/";
            var value = upstream.StandardOutput.Trim();
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(prefix.Length)
                : value;
        }

        private async Task<string> ResolveReleaseTargetBranchAsync()
        {
            await RunGitQuietAsync("fetch", "origin").ConfigureAwait(true);

            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(ReleaseTargetBranch))
                candidates.Add(ReleaseTargetBranch.Trim());

            if (!string.IsNullOrWhiteSpace(DraftBranchName))
                candidates.Add(DraftBranchName.Trim());

            var tracking = await GetUpstreamRemoteBranchNameAsync().ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(tracking))
                candidates.Add(tracking);

            var current = StripRebaseSuffix(CurrentBranch);
            if (!string.IsNullOrWhiteSpace(current))
                candidates.Add(current);

            var main = await GetOriginDefaultBranchAsync().ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(main))
                candidates.Add(main);

            foreach (var branch in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (await RemoteBranchExistsAsync(branch).ConfigureAwait(true))
                    return branch;
            }

            return null;
        }
    }
}

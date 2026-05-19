using System;
using System.Threading;
using System.Threading.Tasks;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.Services
{
    /// <summary>Thin wrapper over <see cref="GitRunner"/> for repo-scoped git commands.</summary>
    public sealed class GitOperationsService
    {
        public Func<string> ResolveWorkingDirectory { get; set; }
        public Func<bool> HasValidRepository { get; set; }
        public Action<GitRunResult, string> LogResult { get; set; }
        public Action<bool> SetBusy { get; set; }
        public Func<bool> IsBusySuppressed { get; set; }
        public Func<CancellationToken> GetCancellationToken { get; set; }

        public async Task<GitRunResult> RunQuietAsync(params string[] args)
        {
            var workDir = ResolveWorkingDirectory?.Invoke();
            if (string.IsNullOrWhiteSpace(workDir))
                return new GitRunResult { ExitCode = -1, StandardError = "No valid repository." };

            var manageBusy = IsBusySuppressed?.Invoke() != true;
            if (manageBusy)
                SetBusy?.Invoke(true);
            try
            {
                var token = GetCancellationToken?.Invoke() ?? CancellationToken.None;
                return await GitRunner.RunAsync(workDir, token, args).ConfigureAwait(false);
            }
            finally
            {
                if (manageBusy)
                    SetBusy?.Invoke(false);
            }
        }

        public async Task<GitRunResult> RunLoggedAsync(params string[] args)
        {
            var result = await RunQuietAsync(args).ConfigureAwait(false);
            LogResult?.Invoke(result, string.Join(" ", args));
            return result;
        }

        public async Task<GitRunResult> RunGlobalConfigAsync(params string[] args)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var manageBusy = IsBusySuppressed?.Invoke() != true;
            if (manageBusy)
                SetBusy?.Invoke(true);
            try
            {
                var token = GetCancellationToken?.Invoke() ?? CancellationToken.None;
                return await GitRunner.RunAsync(home, token, args).ConfigureAwait(false);
            }
            finally
            {
                if (manageBusy)
                    SetBusy?.Invoke(false);
            }
        }
    }
}

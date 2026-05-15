using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.Services
{
    public static class GitHubCliRunner
    {
        private static string _ghExecutable;

        public static string FindGhExecutable()
        {
            if (!string.IsNullOrEmpty(_ghExecutable) && File.Exists(_ghExecutable))
                return _ghExecutable;

            var fromPath = FindOnPath("gh.exe");
            if (fromPath != null)
            {
                _ghExecutable = fromPath;
                return _ghExecutable;
            }

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var candidate = Path.Combine(programFiles, "GitHub CLI", "gh.exe");
            if (File.Exists(candidate))
            {
                _ghExecutable = candidate;
                return _ghExecutable;
            }

            return null;
        }

        public static bool IsAvailable() => FindGhExecutable() != null;

        public static async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken)
        {
            var result = await RunAsync(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                cancellationToken,
                "auth", "status").ConfigureAwait(false);

            if (!result.Success)
                return false;

            var text = (result.StandardOutput + result.StandardError).ToLowerInvariant();
            return text.IndexOf("logged in", StringComparison.Ordinal) >= 0;
        }

        public static async Task<GitRunResult> CreateRepositoryAsync(
            NewRepositoryRequest request,
            CancellationToken cancellationToken)
        {
            var gh = FindGhExecutable();
            if (gh == null)
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "GitHub CLI (gh) not found. Install: https://cli.github.com/"
                };
            }

            if (request == null || string.IsNullOrWhiteSpace(request.FullPath))
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Repository path is empty."
                };
            }

            var args = new List<string> { "repo", "create", request.Name.Trim() };
            args.Add(request.IsPrivate ? "--private" : "--public");

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                args.Add("--description");
                args.Add(request.Description.Trim());
            }

            var create = await RunGhAsync(gh, request.FullPath, cancellationToken, args.ToArray()).ConfigureAwait(false);
            if (!create.Success)
                return create;

            var hasHead = await GitRunner.RunAsync(request.FullPath, cancellationToken, "rev-parse", "HEAD").ConfigureAwait(false);
            if (!hasHead.Success)
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Local repository has no commits. Cannot push to GitHub.",
                    StandardOutput = create.StandardOutput,
                };
            }

            var remoteUrl = await ResolveRemoteUrlAsync(request, create, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(remoteUrl))
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Could not determine GitHub remote URL after creating the repository.",
                    StandardOutput = create.StandardOutput,
                };
            }

            var remote = await GitRunner.RunAsync(request.FullPath, cancellationToken, "remote", "add", "origin", remoteUrl).ConfigureAwait(false);
            if (!remote.Success &&
                remote.StandardError.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) < 0)
            {
                remote = await GitRunner.RunAsync(request.FullPath, cancellationToken, "remote", "set-url", "origin", remoteUrl).ConfigureAwait(false);
                if (!remote.Success)
                {
                    return new GitRunResult
                    {
                        ExitCode = remote.ExitCode,
                        StandardError = remote.StandardError,
                        StandardOutput = create.StandardOutput,
                    };
                }
            }

            var branch = await GitRunner.RunAsync(request.FullPath, cancellationToken, "branch", "--show-current").ConfigureAwait(false);
            var branchName = branch.Success && !string.IsNullOrWhiteSpace(branch.StandardOutput)
                ? branch.StandardOutput.Trim()
                : "main";

            var push = await GitRunner.RunAsync(request.FullPath, cancellationToken, "push", "-u", "origin", branchName).ConfigureAwait(false);
            return new GitRunResult
            {
                ExitCode = push.ExitCode,
                StandardOutput = create.StandardOutput + Environment.NewLine + push.StandardOutput,
                StandardError = push.StandardError,
            };
        }

        private static async Task<string> ResolveRemoteUrlAsync(
            NewRepositoryRequest request,
            GitRunResult createResult,
            CancellationToken cancellationToken)
        {
            var fromOutput = TryParseUrlFromGhOutput(createResult?.StandardOutput);
            if (!string.IsNullOrWhiteSpace(fromOutput))
                return ToGitRemoteUrl(fromOutput);

            var view = await RunAsync(
                request.FullPath,
                cancellationToken,
                "repo", "view", request.Name.Trim(), "--json", "url").ConfigureAwait(false);

            if (view.Success)
            {
                var fromView = TryParseUrlFromGhOutput(view.StandardOutput);
                if (!string.IsNullOrWhiteSpace(fromView))
                    return ToGitRemoteUrl(fromView);
            }

            return null;
        }

        private static string TryParseUrlFromGhOutput(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var start = text.IndexOf("https://github.com/", StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return null;

            var end = text.IndexOfAny(new[] { '"', '\r', '\n', ' ' }, start);
            return end > start ? text.Substring(start, end - start) : text.Substring(start);
        }

        private static string ToGitRemoteUrl(string webUrl)
        {
            webUrl = webUrl.Trim().TrimEnd('/');
            if (webUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                return webUrl;

            return webUrl + ".git";
        }

        public static async Task<GitRunResult> CreateReleaseAsync(
            string repoPath,
            ReleaseRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Tag))
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Release tag is required."
                };
            }

            var args = new List<string> { "release", "create", request.Tag.Trim() };

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                args.Add("--title");
                args.Add(request.Title.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                args.Add("--notes");
                args.Add(request.Notes.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.TargetBranch))
            {
                args.Add("--target");
                args.Add(request.TargetBranch.Trim());
            }

            if (request.IsLatest)
                args.Add("--latest");

            if (request.IsPrerelease)
                args.Add("--prerelease");

            if (request.AssetPaths != null)
            {
                foreach (var asset in request.AssetPaths)
                {
                    if (string.IsNullOrWhiteSpace(asset))
                        continue;

                    var path = Path.GetFullPath(asset.Trim());
                    if (!File.Exists(path))
                    {
                        return new GitRunResult
                        {
                            ExitCode = -1,
                            StandardError = "Release asset not found: " + path
                        };
                    }

                    args.Add(path);
                }
            }

            return await RunAsync(repoPath, cancellationToken, args.ToArray()).ConfigureAwait(false);
        }

        public static async Task<string> TryGetRemoteWebUrlAsync(string repoPath, CancellationToken cancellationToken)
        {
            var result = await RunAsync(repoPath, cancellationToken, "repo", "view", "--json", "url").ConfigureAwait(false);
            if (!result.Success)
                return null;

            var json = result.StandardOutput.Trim();
            var start = json.IndexOf("http", StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return null;

            var end = json.IndexOf('"', start);
            return end > start ? json.Substring(start, end - start) : json.Substring(start).Trim('"', ' ', '\r', '\n');
        }

        private static Task<GitRunResult> RunGhAsync(
            string gh,
            string workingDirectory,
            CancellationToken cancellationToken,
            params string[] arguments) =>
            RunAsync(gh, workingDirectory, cancellationToken, arguments);

        private static async Task<GitRunResult> RunAsync(
            string workingDirectory,
            CancellationToken cancellationToken,
            params string[] arguments)
        {
            var gh = FindGhExecutable();
            if (gh == null)
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "GitHub CLI (gh) not found."
                };
            }

            return await RunGhAsync(gh, workingDirectory, cancellationToken, arguments).ConfigureAwait(false);
        }

        private static async Task<GitRunResult> RunAsync(
            string gh,
            string workingDirectory,
            CancellationToken cancellationToken,
            params string[] arguments)
        {
            var workDir = string.IsNullOrWhiteSpace(workingDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                : workingDirectory;

            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            var psi = new ProcessStartInfo
            {
                FileName = gh,
                WorkingDirectory = workDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            psi.Arguments = BuildArgumentString(arguments);

            using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                        stdout.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                        stderr.AppendLine(e.Data);
                };

                if (!process.Start())
                {
                    return new GitRunResult
                    {
                        ExitCode = -1,
                        StandardError = "Failed to start gh."
                    };
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() =>
                {
                    while (!process.HasExited)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            try { process.Kill(); } catch { /* ignore */ }
                            break;
                        }

                        Thread.Sleep(50);
                    }
                }, cancellationToken).ConfigureAwait(false);

                process.WaitForExit(5000);

                return new GitRunResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = stdout.ToString().Trim(),
                    StandardError = stderr.ToString().Trim(),
                };
            }
        }

        private static string BuildArgumentString(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return "";

            var sb = new StringBuilder();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                    sb.Append(' ');
                sb.Append(QuoteArgument(arguments[i]));
            }

            return sb.ToString();
        }

        private static string QuoteArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return "\"\"";

            if (arg.IndexOfAny(new[] { ' ', '\t', '"', '\r', '\n' }) < 0)
                return arg;

            return "\"" + arg.Replace("\"", "\\\"") + "\"";
        }

        private static string FindOnPath(string fileName)
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
                return null;

            foreach (var dir in pathEnv.Split(';'))
            {
                try
                {
                    var full = Path.Combine(dir.Trim(), fileName);
                    if (File.Exists(full))
                        return full;
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }
    }
}

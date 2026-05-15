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

            args.Add("--source");
            args.Add(request.FullPath);
            args.Add("--remote=origin");
            args.Add("--push");

            return await RunAsync(gh, request.FullPath, cancellationToken, args.ToArray()).ConfigureAwait(false);
        }

        private static async Task<GitRunResult> RunAsync(
            string gh,
            string workingDirectory,
            CancellationToken cancellationToken,
            params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = gh,
                WorkingDirectory = workingDirectory,
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
                if (string.IsNullOrWhiteSpace(dir))
                    continue;

                var full = Path.Combine(dir.Trim(), fileName);
                if (File.Exists(full))
                    return full;
            }

            return null;
        }
    }
}

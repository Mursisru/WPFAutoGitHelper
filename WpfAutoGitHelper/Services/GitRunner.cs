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
    public static class GitRunner
    {
        private static string _gitExecutable;

        public static string FindGitExecutable()
        {
            if (!string.IsNullOrEmpty(_gitExecutable) && File.Exists(_gitExecutable))
                return _gitExecutable;

            var fromPath = FindOnPath("git.exe");
            if (fromPath != null)
            {
                _gitExecutable = fromPath;
                return _gitExecutable;
            }

            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "cmd", "git.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "cmd", "git.exe"),
            };

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                {
                    _gitExecutable = c;
                    return _gitExecutable;
                }
            }

            return null;
        }

        public static bool IsGitRepository(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return false;
            return Directory.Exists(Path.Combine(directory, ".git"));
        }

        public static async Task<GitRunResult> RunAsync(
            string workingDirectory,
            CancellationToken cancellationToken,
            params string[] arguments)
        {
            var git = FindGitExecutable();
            if (git == null)
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Git not found. Install Git for Windows: https://git-scm.com/download/win"
                };
            }

            if (string.IsNullOrWhiteSpace(workingDirectory) || !Directory.Exists(workingDirectory))
            {
                return new GitRunResult
                {
                    ExitCode = -1,
                    StandardError = "Specify an existing repository folder."
                };
            }

            var psi = new ProcessStartInfo
            {
                FileName = git,
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
                        StandardError = "Failed to start git."
                    };
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var cancelled = false;
                await Task.Run(() =>
                {
                    while (!process.HasExited)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cancelled = true;
                            try { if (!process.HasExited) process.Kill(); } catch { /* ignore */ }
                            if (!process.WaitForExit(5000))
                                try { process.Kill(); } catch { /* ignore */ }
                            break;
                        }
                        Thread.Sleep(50);
                    }
                }, cancellationToken).ConfigureAwait(false);

                if (!process.HasExited)
                    process.WaitForExit(cancelled ? 3000 : 5000);

                return new GitRunResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = stdout.ToString().TrimEnd(),
                    StandardError = stderr.ToString().TrimEnd(),
                };
            }
        }

        public static List<ChangedFileEntry> ParsePorcelain(string porcelainOutput)
        {
            var list = new List<ChangedFileEntry>();
            if (string.IsNullOrWhiteSpace(porcelainOutput))
                return list;

            foreach (var line in porcelainOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length < 3)
                    continue;

                string code;
                string path;
                if (line.StartsWith("??", StringComparison.Ordinal))
                {
                    code = "??";
                    path = line.Substring(3).Trim();
                }
                else
                {
                    code = line.Substring(0, 2).TrimEnd();
                    path = line.Substring(3).Trim();
                }

                if (!string.IsNullOrEmpty(path))
                    list.Add(new ChangedFileEntry { StatusCode = code, FilePath = path });
            }

            return list;
        }

        public static string ToGitHubWebUrl(string remoteUrl)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                return null;

            remoteUrl = remoteUrl.Trim();
            if (remoteUrl.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
            {
                var path = remoteUrl.Substring("git@github.com:".Length).TrimEnd('/');
                if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(0, path.Length - 4);
                return "https://github.com/" + path;
            }

            if (Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri) &&
                uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
            {
                var path = uri.AbsolutePath.Trim('/');
                if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(0, path.Length - 4);
                return "https://github.com/" + path;
            }

            return remoteUrl;
        }

        public static string ToGitRemoteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = url.Trim().TrimEnd('/');
            if (url.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
                return url;

            if (!url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                url += ".git";
            return url;
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
                    // ignore invalid path segments
                }
            }

            return null;
        }
    }
}

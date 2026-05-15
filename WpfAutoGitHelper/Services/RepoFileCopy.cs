using System;
using System.IO;

namespace WpfAutoGitHelper.Services
{
    internal static class RepoFileCopy
    {
        private static readonly string[] SkipDirNames =
        {
            ".git", ".vs", "bin", "obj", "node_modules",
        };

        public static int CopyDirectoryContents(string sourceDir, string destDir)
        {
            if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
                return 0;

            Directory.CreateDirectory(destDir);
            var count = 0;

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var name = Path.GetFileName(file);
                if (string.IsNullOrEmpty(name))
                    continue;

                File.Copy(file, Path.Combine(destDir, name), overwrite: true);
                count++;
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var name = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(name) || ShouldSkipDirectory(name))
                    continue;

                count += CopyDirectoryContents(dir, Path.Combine(destDir, name));
            }

            return count;
        }

        private static bool ShouldSkipDirectory(string name) =>
            Array.Exists(SkipDirNames, n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));

        public static string FindContainingGitRoot(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            try
            {
                var dir = new DirectoryInfo(Path.GetFullPath(directory));
                while (dir != null)
                {
                    if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                        return dir.FullName;
                    dir = dir.Parent;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}

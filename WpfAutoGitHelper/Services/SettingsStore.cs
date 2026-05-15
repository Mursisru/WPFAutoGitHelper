using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace WpfAutoGitHelper.Services
{
    public sealed class AppSettings
    {
        public string RepoPath { get; set; } = "";
        public string LastCommitMessage { get; set; } = "";
        public string CachedGitHubUrl { get; set; } = "";
        public List<string> RecentRepoPaths { get; set; } = new List<string>();
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "light";
        public bool ConfirmCommit { get; set; } = true;
        public bool ConfirmRestore { get; set; } = true;
        public bool AutoRefreshOnSaveRepo { get; set; } = true;
    }

    public static class SettingsStore
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WpfAutoGitHelper");

        private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return TryMigrateFromLegacy() ?? new AppSettings();

                var json = File.ReadAllText(SettingsFile);
                var settings = Serializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        /// <summary>Import settings from older GlocGitHelper if present.</summary>
        private static AppSettings TryMigrateFromLegacy()
        {
            try
            {
                var legacyFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GlocGitHelper",
                    "settings.json");

                if (!File.Exists(legacyFile))
                    return null;

                var json = File.ReadAllText(legacyFile);
                var settings = Serializer.Deserialize<AppSettings>(json);
                if (settings == null)
                    return null;

                Save(settings);
                return settings;
            }
            catch
            {
                return null;
            }
        }

        public static void Save(AppSettings settings)
        {
            if (settings == null)
                return;

            Directory.CreateDirectory(SettingsDir);
            var json = Serializer.Serialize(settings);
            File.WriteAllText(SettingsFile, json);
        }

        public static void RememberRepo(AppSettings settings, string repoPath)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
                return;

            repoPath = Path.GetFullPath(repoPath.Trim());
            settings.RepoPath = repoPath;
            settings.RecentRepoPaths = settings.RecentRepoPaths ?? new List<string>();
            settings.RecentRepoPaths.RemoveAll(p => string.Equals(p, repoPath, StringComparison.OrdinalIgnoreCase));
            settings.RecentRepoPaths.Insert(0, repoPath);
            if (settings.RecentRepoPaths.Count > 12)
                settings.RecentRepoPaths = settings.RecentRepoPaths.Take(12).ToList();
        }
    }
}

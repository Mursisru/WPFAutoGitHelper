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
        public string AccentColor { get; set; } = "blue";
        public string BackgroundColor { get; set; } = "default";
        public bool ConfirmCommit { get; set; } = true;
        public bool ConfirmRestore { get; set; } = true;
        public bool AutoRefreshOnSaveRepo { get; set; } = true;
        public string LastReleaseTag { get; set; } = "";
        public string LastReleaseTitle { get; set; } = "";
        public string LastReleaseNotes { get; set; } = "";
        public List<string> LastReleaseAssetPaths { get; set; } = new List<string>();
    }

    public static class SettingsStore
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static string SettingsDirectory => AppPaths.DataDirectory;

        public static string SettingsFilePath => AppPaths.SettingsFile;

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                    return DeserializeFile(SettingsFilePath) ?? new AppSettings();

                var migrated = TryMigrateFromAppData() ?? TryMigrateFromLegacy();
                return migrated ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            if (settings == null)
                return;

            Directory.CreateDirectory(SettingsDirectory);
            var json = Serializer.Serialize(settings);
            File.WriteAllText(SettingsFilePath, json);
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

        private static AppSettings TryMigrateFromAppData()
        {
            var appDataFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WpfAutoGitHelper",
                "settings.json");

            return MigrateFromFile(appDataFile);
        }

        private static AppSettings TryMigrateFromLegacy()
        {
            var legacyFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GlocGitHelper",
                "settings.json");

            return MigrateFromFile(legacyFile);
        }

        private static AppSettings MigrateFromFile(string sourceFile)
        {
            try
            {
                if (!File.Exists(sourceFile))
                    return null;

                var settings = DeserializeFile(sourceFile);
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

        private static AppSettings DeserializeFile(string path)
        {
            var json = File.ReadAllText(path);
            return Serializer.Deserialize<AppSettings>(json);
        }
    }
}

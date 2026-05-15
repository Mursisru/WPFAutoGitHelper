using System;
using System.IO;
using System.Reflection;

namespace WpfAutoGitHelper.Services
{
    /// <summary>Portable paths next to the running executable.</summary>
    public static class AppPaths
    {
        public static string ExeDirectory
        {
            get
            {
                var location = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(location))
                {
                    var dir = Path.GetDirectoryName(location);
                    if (!string.IsNullOrEmpty(dir))
                        return dir;
                }

                return AppDomain.CurrentDomain.BaseDirectory.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            }
        }

        /// <summary>Folder beside the .exe for settings and other app data files.</summary>
        public static string DataDirectory => Path.Combine(ExeDirectory, "Data");

        public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");
    }
}

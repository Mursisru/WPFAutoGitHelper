using System;
using System.Linq;
using System.Windows;

namespace WpfAutoGitHelper.Services
{
    public static class ThemeManager
    {
        public const string Light = "light";
        public const string Dark = "dark";

        public static event Action ThemeChanged;

        public static void Apply(string theme)
        {
            theme = Normalize(theme);

            var app = Application.Current;
            if (app == null)
                return;

            ApplyToResources(app.Resources, theme);

            foreach (Window window in app.Windows)
                ApplyToResources(window.Resources, theme);

            ThemeChanged?.Invoke();
        }

        public static void ApplyTo(Window window, string theme)
        {
            if (window == null)
                return;

            ApplyToResources(window.Resources, Normalize(theme));
        }

        private static string Normalize(string theme)
        {
            theme = string.IsNullOrWhiteSpace(theme) ? Light : theme.Trim().ToLowerInvariant();
            return theme == Dark ? Dark : Light;
        }

        private static void ApplyToResources(ResourceDictionary resources, string theme)
        {
            var themeUri = new Uri(
                theme == Dark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml",
                UriKind.Relative);

            var dictionaries = resources.MergedDictionaries;
            var existing = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.Contains("LightTheme.xaml") ||
                 d.Source.OriginalString.Contains("DarkTheme.xaml")));

            if (existing != null)
                dictionaries.Remove(existing);

            dictionaries.Insert(0, new ResourceDictionary { Source = themeUri });
        }
    }
}

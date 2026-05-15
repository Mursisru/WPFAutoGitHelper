using System;
using System.Linq;
using System.Windows;

namespace WpfAutoGitHelper.Services
{
    public static class ThemeManager
    {
        public const string Light = "light";
        public const string Dark = "dark";
        public const string Black = "black";

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

        public static string Normalize(string theme)
        {
            theme = string.IsNullOrWhiteSpace(theme) ? Light : theme.Trim().ToLowerInvariant();
            if (theme == Black)
                return Black;
            if (theme == Dark)
                return Dark;
            return Light;
        }

        private static void ApplyToResources(ResourceDictionary resources, string theme)
        {
            var themeUri = GetThemeUri(theme);

            var dictionaries = resources.MergedDictionaries;
            var existing = dictionaries.FirstOrDefault(IsThemeDictionary);

            if (existing != null)
                dictionaries.Remove(existing);

            dictionaries.Insert(0, new ResourceDictionary { Source = themeUri });
        }

        private static Uri GetThemeUri(string theme)
        {
            string path;
            if (theme == Black)
                path = "Themes/BlackTheme.xaml";
            else if (theme == Dark)
                path = "Themes/DarkTheme.xaml";
            else
                path = "Themes/LightTheme.xaml";

            return new Uri(path, UriKind.Relative);
        }

        private static bool IsThemeDictionary(ResourceDictionary dictionary)
        {
            if (dictionary?.Source == null)
                return false;

            var source = dictionary.Source.OriginalString;
            return source.IndexOf("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) >= 0
                || source.IndexOf("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) >= 0
                || source.IndexOf("BlackTheme.xaml", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

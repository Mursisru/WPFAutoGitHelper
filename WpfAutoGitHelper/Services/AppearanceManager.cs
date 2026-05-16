using System.Windows;

namespace WpfAutoGitHelper.Services
{
    /// <summary>Applies theme, accent, and background overrides once on app resources (no re-entrant events).</summary>
    public static class AppearanceManager
    {
        private static bool _applying;
        private static string _lastTheme;
        private static string _lastAccent;
        private static string _lastBackground;

        public static void Apply(string themeId, string accentId, string backgroundId)
        {
            themeId = ThemeManager.Normalize(themeId);
            accentId = AccentPalette.NormalizeId(accentId);
            backgroundId = BackgroundPalette.NormalizeId(backgroundId);

            if (_applying)
                return;

            if (string.Equals(_lastTheme, themeId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(_lastAccent, accentId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(_lastBackground, backgroundId, System.StringComparison.OrdinalIgnoreCase))
                return;

            _applying = true;
            try
            {
                var app = Application.Current;
                if (app == null)
                    return;

                ThemeManager.ApplyToResources(app.Resources, themeId);
                AccentPalette.ApplyToResources(app.Resources, accentId, themeId);
                BackgroundPalette.ApplyToResources(app.Resources, backgroundId, themeId);

                _lastTheme = themeId;
                _lastAccent = accentId;
                _lastBackground = backgroundId;
            }
            finally
            {
                _applying = false;
            }
        }

        public static void ApplyTo(Window window, string themeId, string accentId, string backgroundId)
        {
            if (window == null)
                return;

            themeId = ThemeManager.Normalize(themeId);
            accentId = AccentPalette.NormalizeId(accentId);
            backgroundId = BackgroundPalette.NormalizeId(backgroundId);

            ThemeManager.ApplyToResources(window.Resources, themeId);
            AccentPalette.ApplyToResources(window.Resources, accentId, themeId);
            BackgroundPalette.ApplyToResources(window.Resources, backgroundId, themeId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WpfAutoGitHelper.Services
{
    public static class AccentPalette
    {
        public const string DefaultId = "blue";

        public static readonly string[] AllIds =
        {
            "blue", "teal", "green", "orange", "purple", "rose", "amber",
            "red", "cyan", "lime", "indigo", "pink", "gold", "sky",
        };

        public static void ApplyToResources(ResourceDictionary resources, string accentId, string themeId)
        {
            if (resources == null)
                return;

            var colors = Resolve(accentId, themeId);
            ApplyToResources(resources, colors);
        }

        private static void ApplyToResources(ResourceDictionary resources, AccentColors colors)
        {
            if (resources == null)
                return;

            resources["BrushAccent"] = MakeBrushSafe(colors.Accent);
            resources["BrushAccentHover"] = MakeBrushSafe(colors.Hover);
            resources["BrushAccentSoft"] = MakeBrushSafe(colors.Soft);
            resources["ColorAccent"] = colors.Accent;
            resources["ColorAccentHover"] = colors.Hover;
            resources["ColorAccentSoft"] = colors.Soft;
        }

        private static AccentColors Resolve(string accentId, string themeId)
        {
            accentId = NormalizeId(accentId);
            var dark = themeId == ThemeManager.Dark || themeId == ThemeManager.Black;
            return dark ? GetDark(accentId) : GetLight(accentId);
        }

        public static string NormalizeId(string accentId)
        {
            if (string.IsNullOrWhiteSpace(accentId))
                return DefaultId;

            accentId = accentId.Trim().ToLowerInvariant();
            foreach (var id in AllIds)
            {
                if (string.Equals(id, accentId, StringComparison.Ordinal))
                    return id;
            }

            return DefaultId;
        }

        private static SolidColorBrush MakeBrushSafe(Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();
            return brush;
        }

        private static AccentColors GetLight(string id)
        {
            switch (id)
            {
                case "teal": return new AccentColors(Parse("#0D9488"), Parse("#0F766E"), Parse("#CCFBF1"));
                case "green": return new AccentColors(Parse("#16A34A"), Parse("#15803D"), Parse("#DCFCE7"));
                case "orange": return new AccentColors(Parse("#EA580C"), Parse("#C2410C"), Parse("#FFEDD5"));
                case "purple": return new AccentColors(Parse("#7C3AED"), Parse("#6D28D9"), Parse("#EDE9FE"));
                case "rose": return new AccentColors(Parse("#E11D48"), Parse("#BE123C"), Parse("#FFE4E6"));
                case "amber": return new AccentColors(Parse("#D97706"), Parse("#B45309"), Parse("#FEF3C7"));
                case "red": return new AccentColors(Parse("#DC2626"), Parse("#B91C1C"), Parse("#FEE2E2"));
                case "cyan": return new AccentColors(Parse("#0891B2"), Parse("#0E7490"), Parse("#CFFAFE"));
                case "lime": return new AccentColors(Parse("#65A30D"), Parse("#4D7C0F"), Parse("#ECFCCB"));
                case "indigo": return new AccentColors(Parse("#4F46E5"), Parse("#4338CA"), Parse("#E0E7FF"));
                case "pink": return new AccentColors(Parse("#DB2777"), Parse("#BE185D"), Parse("#FCE7F3"));
                case "gold": return new AccentColors(Parse("#CA8A04"), Parse("#A16207"), Parse("#FEF9C3"));
                case "sky": return new AccentColors(Parse("#0284C7"), Parse("#0369A1"), Parse("#E0F2FE"));
                default: return new AccentColors(Parse("#2563EB"), Parse("#1D4ED8"), Parse("#DBEAFE"));
            }
        }

        private static AccentColors GetDark(string id)
        {
            switch (id)
            {
                case "teal": return new AccentColors(Parse("#2DD4BF"), Parse("#5EEAD4"), Parse("#134E4A"));
                case "green": return new AccentColors(Parse("#4ADE80"), Parse("#86EFAC"), Parse("#14532D"));
                case "orange": return new AccentColors(Parse("#FB923C"), Parse("#FDBA74"), Parse("#7C2D12"));
                case "purple": return new AccentColors(Parse("#A78BFA"), Parse("#C4B5FD"), Parse("#1E1B2E"));
                case "rose": return new AccentColors(Parse("#FB7185"), Parse("#FDA4AF"), Parse("#4C0519"));
                case "amber": return new AccentColors(Parse("#FBBF24"), Parse("#FDE047"), Parse("#451A03"));
                case "red": return new AccentColors(Parse("#F87171"), Parse("#FCA5A5"), Parse("#450A0A"));
                case "cyan": return new AccentColors(Parse("#22D3EE"), Parse("#67E8F9"), Parse("#083344"));
                case "lime": return new AccentColors(Parse("#A3E635"), Parse("#BEF264"), Parse("#1A2E05"));
                case "indigo": return new AccentColors(Parse("#818CF8"), Parse("#A5B4FC"), Parse("#1E1B4B"));
                case "pink": return new AccentColors(Parse("#F472B6"), Parse("#F9A8D4"), Parse("#500724"));
                case "gold": return new AccentColors(Parse("#FACC15"), Parse("#FDE047"), Parse("#422006"));
                case "sky": return new AccentColors(Parse("#38BDF8"), Parse("#7DD3FC"), Parse("#0C4A6E"));
                default: return new AccentColors(Parse("#38BDF8"), Parse("#7DD3FC"), Parse("#0C4A6E"));
            }
        }

        private static Color Parse(string hex) =>
            (Color)ColorConverter.ConvertFromString(hex);

        private sealed class AccentColors
        {
            public AccentColors(Color accent, Color hover, Color soft)
            {
                Accent = accent;
                Hover = hover;
                Soft = soft;
            }

            public Color Accent { get; }
            public Color Hover { get; }
            public Color Soft { get; }
        }
    }
}

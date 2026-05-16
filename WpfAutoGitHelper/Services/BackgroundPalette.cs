using System;
using System.Windows;
using System.Windows.Media;

namespace WpfAutoGitHelper.Services
{
    public static class BackgroundPalette
    {
        public const string DefaultId = "default";

        public static readonly string[] AllIds =
        {
            DefaultId, "cool", "warm", "mint", "navy", "graphite", "espresso",
            "stone", "ocean", "plum", "dusk", "slate", "cherry",
        };

        public static string NormalizeId(string backgroundId)
        {
            if (string.IsNullOrWhiteSpace(backgroundId))
                return DefaultId;

            backgroundId = backgroundId.Trim().ToLowerInvariant();
            foreach (var id in AllIds)
            {
                if (string.Equals(id, backgroundId, StringComparison.Ordinal))
                    return id;
            }

            return DefaultId;
        }

        public static void ApplyToResources(ResourceDictionary resources, string backgroundId, string themeId)
        {
            if (resources == null)
                return;

            backgroundId = NormalizeId(backgroundId);
            if (backgroundId == DefaultId)
                return;

            var colors = Resolve(backgroundId, themeId);
            if (colors == null)
                return;

            SetBrush(resources, "BrushBg", colors.Bg);
            SetBrush(resources, "BrushSurface", colors.Surface);
            SetBrush(resources, "BrushCardBg", colors.Card);
            SetBrush(resources, "BrushTabInactive", colors.TabInactive);
            SetBrush(resources, "BrushTabHover", colors.TabHover);
            SetBrush(resources, "BrushTabSelected", colors.TabSelected);
            SetBrush(resources, "BrushInputBg", colors.InputBg);
            SetBrush(resources, "BrushSecondaryBtn", colors.SecondaryBtn);
            SetBrush(resources, "BrushHelpPanelBg", colors.HelpPanel);
            SetBrush(resources, "BrushScrollTrack", colors.ScrollTrack);

            resources["ColorBg"] = colors.Bg;
            resources["ColorSurface"] = colors.Surface;
            resources["ColorCardBg"] = colors.Card;
        }

        private static void SetBrush(ResourceDictionary resources, string key, Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();
            resources[key] = brush;
        }

        private static BackgroundColors Resolve(string backgroundId, string themeId)
        {
            var dark = themeId == ThemeManager.Dark || themeId == ThemeManager.Black;
            return dark ? GetDark(backgroundId) : GetLight(backgroundId);
        }

        private static BackgroundColors GetLight(string id)
        {
            switch (id)
            {
                case "cool":
                    return new BackgroundColors("#E8EEF5", "#F8FAFC", "#FFFFFF", "#E2E8F0", "#D8E2EC", "#FFFFFF", "#FFFFFF", "#F1F5F9", "#F1F5F9", "#E2E8F0");
                case "warm":
                    return new BackgroundColors("#F3EDE4", "#FAF6F0", "#FFFCF8", "#E8DFD4", "#DDD2C4", "#FFFCF8", "#FFFCF8", "#F5EFE6", "#F5EFE6", "#E8DFD4");
                case "mint":
                    return new BackgroundColors("#E6F4EF", "#F2FAF7", "#FFFFFF", "#D5EBE3", "#C5E0D6", "#FFFFFF", "#FFFFFF", "#EAF6F1", "#EAF6F1", "#D5EBE3");
                case "navy":
                    return new BackgroundColors("#E4E9F2", "#EEF2F8", "#F8FAFC", "#D5DCE8", "#C5CEDC", "#F8FAFC", "#F8FAFC", "#E8EDF5", "#E8EDF5", "#D5DCE8");
                case "graphite":
                    return new BackgroundColors("#ECECEC", "#F5F5F5", "#FFFFFF", "#DDDDDD", "#D0D0D0", "#FFFFFF", "#FFFFFF", "#EFEFEF", "#EFEFEF", "#DDDDDD");
                case "espresso":
                    return new BackgroundColors("#EDE8E4", "#F7F3F0", "#FFFBF8", "#E0D8D2", "#D4C9C0", "#FFFBF8", "#FFFBF8", "#F0EBE6", "#F0EBE6", "#E0D8D2");
                case "stone":
                    return new BackgroundColors("#E7E5E4", "#F5F5F4", "#FFFFFF", "#D6D3D1", "#C8C4C0", "#FFFFFF", "#FFFFFF", "#EDEBE9", "#EDEBE9", "#D6D3D1");
                case "ocean":
                    return new BackgroundColors("#E0F2FE", "#F0F9FF", "#FFFFFF", "#BAE6FD", "#9BD5F5", "#FFFFFF", "#FFFFFF", "#E8F6FC", "#E8F6FC", "#BAE6FD");
                case "plum":
                    return new BackgroundColors("#F3E8FF", "#FAF5FF", "#FFFFFF", "#E9D5FF", "#D8B4FE", "#FFFFFF", "#FFFFFF", "#F5EDFF", "#F5EDFF", "#E9D5FF");
                case "dusk":
                    return new BackgroundColors("#E8EAF2", "#F1F3F9", "#FFFFFF", "#D5DAE8", "#C5CCDC", "#FFFFFF", "#FFFFFF", "#ECEFF6", "#ECEFF6", "#D5DAE8");
                case "slate":
                    return new BackgroundColors("#E2E8F0", "#F1F5F9", "#FFFFFF", "#CBD5E1", "#B8C4D4", "#FFFFFF", "#FFFFFF", "#E8EDF3", "#E8EDF3", "#CBD5E1");
                case "cherry":
                    return new BackgroundColors("#FCE7F3", "#FFF1F5", "#FFFFFF", "#FBCFE8", "#F9A8D4", "#FFFFFF", "#FFFFFF", "#FDF2F8", "#FDF2F8", "#FBCFE8");
                default:
                    return null;
            }
        }

        private static BackgroundColors GetDark(string id)
        {
            switch (id)
            {
                case "cool":
                    return new BackgroundColors("#0B1120", "#111827", "#1A2332", "#151D2B", "#243044", "#1E293B", "#0F172A", "#243044", "#151D2B", "#1E293B");
                case "warm":
                    return new BackgroundColors("#1A1410", "#231C18", "#2E2620", "#1F1915", "#332A24", "#2A221C", "#1A1410", "#332A24", "#1F1915", "#2A221C");
                case "mint":
                    return new BackgroundColors("#0A1512", "#0F1F1A", "#152A24", "#0D1A16", "#1A3530", "#152A24", "#0A1512", "#1A3530", "#0D1A16", "#152820");
                case "navy":
                    return new BackgroundColors("#060B14", "#0C1220", "#151D30", "#0A101C", "#182440", "#151D30", "#060B14", "#182440", "#0A101C", "#101828");
                case "graphite":
                    return new BackgroundColors("#181818", "#222222", "#2A2A2A", "#141414", "#303030", "#2A2A2A", "#181818", "#303030", "#141414", "#282828");
                case "espresso":
                    return new BackgroundColors("#140F0C", "#1C1612", "#261E19", "#181210", "#302620", "#261E19", "#140F0C", "#302620", "#181210", "#221A15");
                case "stone":
                    return new BackgroundColors("#1C1917", "#292524", "#35302E", "#171412", "#3D3835", "#35302E", "#1C1917", "#3D3835", "#171412", "#2E2A28");
                case "ocean":
                    return new BackgroundColors("#0C1929", "#0F2438", "#15304A", "#0A1520", "#1A3F5C", "#15304A", "#0C1929", "#1A3F5C", "#0A1520", "#122A3D");
                case "plum":
                    return new BackgroundColors("#1A0F24", "#241530", "#302040", "#140A1C", "#3D2850", "#302040", "#1A0F24", "#3D2850", "#140A1C", "#281C38");
                case "dusk":
                    return new BackgroundColors("#12141F", "#1A1D2B", "#242838", "#0E1018", "#2E3348", "#242838", "#12141F", "#2E3348", "#0E1018", "#1E2230");
                case "slate":
                    return new BackgroundColors("#0F172A", "#1E293B", "#273449", "#0B1220", "#334155", "#273449", "#0F172A", "#334155", "#0B1220", "#1E293B");
                case "cherry":
                    return new BackgroundColors("#1F0A12", "#2A1018", "#381520", "#18060E", "#4A1A2C", "#381520", "#1F0A12", "#4A1A2C", "#18060E", "#2C101C");
                default:
                    return null;
            }
        }

        private static Color Parse(string hex) => (Color)ColorConverter.ConvertFromString(hex);

        private sealed class BackgroundColors
        {
            public BackgroundColors(string bg, string surface, string card, string tabInactive, string tabHover,
                string tabSelected, string inputBg, string secondaryBtn, string helpPanel, string scrollTrack)
            {
                Bg = Parse(bg);
                Surface = Parse(surface);
                Card = Parse(card);
                TabInactive = Parse(tabInactive);
                TabHover = Parse(tabHover);
                TabSelected = Parse(tabSelected);
                InputBg = Parse(inputBg);
                SecondaryBtn = Parse(secondaryBtn);
                HelpPanel = Parse(helpPanel);
                ScrollTrack = Parse(scrollTrack);
            }

            public Color Bg { get; }
            public Color Surface { get; }
            public Color Card { get; }
            public Color TabInactive { get; }
            public Color TabHover { get; }
            public Color TabSelected { get; }
            public Color InputBg { get; }
            public Color SecondaryBtn { get; }
            public Color HelpPanel { get; }
            public Color ScrollTrack { get; }
        }
    }
}

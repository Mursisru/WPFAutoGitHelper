using System;
using System.Collections.Generic;
namespace WpfAutoGitHelper.Localization
{
    public static class Loc
    {
        public const string DefaultLanguage = "en";

        public static event Action LanguageChanged;

        private static string _language = DefaultLanguage;

        public static string Language
        {
            get => _language;
            set => ApplyLanguage(value);
        }

        /// <summary>Sets language and always notifies listeners (for UI refresh).</summary>
        public static void ApplyLanguage(string code)
        {
            code = Normalize(code);
            _language = code;
            LanguageChanged?.Invoke();
        }

        public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[]
        {
            new LanguageOption("en", "English"),
            new LanguageOption("ru", "Русский"),
            new LanguageOption("uk", "Українська"),
            new LanguageOption("de", "Deutsch"),
            new LanguageOption("fr", "Français"),
            new LanguageOption("es", "Español"),
            new LanguageOption("pt", "Português"),
            new LanguageOption("pl", "Polski"),
            new LanguageOption("it", "Italiano"),
            new LanguageOption("nl", "Nederlands"),
            new LanguageOption("tr", "Türkçe"),
            new LanguageOption("zh", "中文"),
            new LanguageOption("ja", "日本語"),
            new LanguageOption("ko", "한국어"),
            new LanguageOption("cs", "Čeština"),
        };

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (TryGetFrom(_language, key, out var value))
                return value;

            if (_language != DefaultLanguage && TryGetFrom(DefaultLanguage, key, out value))
                return value;

            return key;
        }

        public static string Normalize(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return DefaultLanguage;

            code = code.Trim().ToLowerInvariant();
            if (code.Length > 2)
                code = code.Substring(0, 2);

            return LocResources.Tables.ContainsKey(code) ? code : DefaultLanguage;
        }

        private static bool TryGetFrom(string language, string key, out string value)
        {
            value = null;
            if (!LocResources.Tables.TryGetValue(language, out var table) || table == null)
                return false;

            return table.TryGetValue(key, out value) && value != null;
        }
    }

    public sealed class LanguageOption
    {
        public LanguageOption(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }

        public string Code { get; }
        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}

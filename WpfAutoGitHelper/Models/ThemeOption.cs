namespace WpfAutoGitHelper.Models
{
    public sealed class ThemeOption
    {
        public ThemeOption(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}

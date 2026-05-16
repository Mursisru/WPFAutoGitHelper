namespace WpfAutoGitHelper.Models
{
    public sealed class AccentColorOption
    {
        public AccentColorOption(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}

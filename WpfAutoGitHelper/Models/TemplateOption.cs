namespace WpfAutoGitHelper.Models
{
    public sealed class TemplateOption
    {
        public TemplateOption(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}

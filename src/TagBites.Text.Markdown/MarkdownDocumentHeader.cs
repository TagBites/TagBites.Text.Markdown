namespace TagBites.Text.Markdown
{
    public class MarkdownDocumentHeader
    {
        private readonly IDictionary<string, string> _metadata;

        public string? Uid => this["uid"];
        public string? Title => this["title"];

        public string? this[string name] => _metadata.TryGetValue(name, out var v) ? v : null;

        public MarkdownDocumentHeader(IDictionary<string, string> metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }
}

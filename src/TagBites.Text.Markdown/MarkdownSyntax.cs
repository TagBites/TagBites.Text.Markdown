namespace TagBites.Text.Markdown
{
    public static class MarkdownSyntax
    {
        public static string Bold(string text) => string.IsNullOrEmpty(text) ? string.Empty : $"**{text}**";
        public static string Italic(string text) => string.IsNullOrEmpty(text) ? string.Empty : $"_{text}_";
        public static string Strikethrough(string text) => string.IsNullOrEmpty(text) ? string.Empty : $"~~{text}~~";
        public static string Code(string code) => string.IsNullOrEmpty(code) ? string.Empty : $"`{code}`";
        public static string Link(string name, string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address can not be null or empty.", nameof(address));

            return $"[{name}]({address})";
        }
        public static string Image(string name, string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address can not be null or empty.", nameof(address));

            return $"![{name}]({address})";
        }
        public static string EscapeHtml(string html) => string.IsNullOrEmpty(html) ? string.Empty : html.Replace("<", "&lt;").Replace(">", "&gt;");
    }
}

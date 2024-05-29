namespace TagBites.Text.Markdown
{
    public class MarkdownParagraph : MarkdownElement
    {
        public string Text { get; }

        public MarkdownParagraph(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }


        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            builder.Append(Text);
        }
    }
}

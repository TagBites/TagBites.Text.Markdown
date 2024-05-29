namespace TagBites.Text.Markdown
{
    public class MarkdownQuote : MarkdownElement
    {
        public string Text { get; }

        public MarkdownQuote(string text) => Text = text;


        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            if (!builder.CleanTextMode)
                builder.Append("> ");
            builder.Append(Text);
        }
    }
}

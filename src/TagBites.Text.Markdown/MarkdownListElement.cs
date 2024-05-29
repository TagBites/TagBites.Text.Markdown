namespace TagBites.Text.Markdown
{
    public class MarkdownListElement : MarkdownContentElement
    {
        public string Text { get; }

        public MarkdownListElement(string text) => Text = text;


        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            builder.Append(Text);
            builder.AppendLine();

            builder.PushIndent(builder.Indent + 1);
            base.Resolve(builder);
            builder.PopIndent();
        }
    }
}

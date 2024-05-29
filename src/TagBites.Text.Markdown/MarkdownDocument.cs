namespace TagBites.Text.Markdown
{
    public class MarkdownDocument : MarkdownContentElement
    {
        public new IList<MarkdownElement> Content => base.Content;


        public MarkdownDocument WithHeader(int level, string text)
        {
            AddHeader(level, text);
            return this;
        }
        public MarkdownDocument WithHeader(int level, string text, string customId)
        {
            AddHeader(level, text).WithCustomId(customId);
            return this;
        }
        public MarkdownHeader AddHeader(int level, string text) => AddCore(new MarkdownHeader(level, text));
    }
}

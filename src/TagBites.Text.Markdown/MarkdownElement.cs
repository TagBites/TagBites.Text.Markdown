namespace TagBites.Text.Markdown
{
    public abstract class MarkdownElement
    {
        protected internal abstract void Resolve(MarkdownStringBuilder builder);

        public override string ToString() => ToString(false);
        public string ToString(bool cleanTextMode)
        {
            var sb = new MarkdownStringBuilder(cleanTextMode);
            Resolve(sb);
            return sb.ToString();
        }
    }
}

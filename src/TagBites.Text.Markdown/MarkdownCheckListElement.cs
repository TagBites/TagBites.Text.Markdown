namespace TagBites.Text.Markdown
{
    public class MarkdownCheckListElement : MarkdownListElement
    {
        public bool IsChecked { get; }

        public MarkdownCheckListElement(bool isChecked, string text)
            : base(text)
        {
            IsChecked = isChecked;
        }


        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            builder.Append(IsChecked ? "[x] " : "[ ] ");
            base.Resolve(builder);
        }
    }
}

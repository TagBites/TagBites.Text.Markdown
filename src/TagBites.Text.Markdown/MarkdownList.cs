namespace TagBites.Text.Markdown
{
    public class MarkdownList : MarkdownElement
    {
        protected virtual bool IsOrdered => false;
        protected virtual bool IsCheckList => false;

        public List<MarkdownListElement> Elements { get; } = new();


        public MarkdownListElement AddElement(string text)
        {
            var element = new MarkdownListElement(text);
            Elements.Add(element);
            return element;
        }

        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            for (var i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];

                if (IsOrdered)
                {
                    builder.Append((i + 1).ToString());
                    builder.Append(". ");
                }
                else
                    builder.Append("- ");

                element.Resolve(builder);
            }
        }
    }
}

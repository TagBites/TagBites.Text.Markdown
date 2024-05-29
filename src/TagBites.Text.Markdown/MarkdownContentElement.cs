namespace TagBites.Text.Markdown
{
    public abstract class MarkdownContentElement : MarkdownElement
    {
        private IList<MarkdownElement>? _content;

        protected IList<MarkdownElement> Content => _content ??= new List<MarkdownElement>();


        public MarkdownParagraph AddParagraph(string text) => AddCore(new MarkdownParagraph(text));
        public MarkdownCode AddCode(string code) => AddCore(new MarkdownCode(null, code));
        public MarkdownCode AddCode(string language, string code) => AddCore(new MarkdownCode(language, code));
        public MarkdownQuote AddQuote(string quote) => AddCore(new MarkdownQuote(quote));
        public MarkdownList AddList() => AddCore(new MarkdownList());
        public MarkdownOrderedList AddOrderedList() => AddCore(new MarkdownOrderedList());
        public MarkdownCheckList AddCheckList() => AddCore(new MarkdownCheckList());
        public MarkdownTable AddTable() => AddCore(new MarkdownTable());

        protected internal T AddCore<T>(T element) where T : MarkdownElement
        {
            Content.Add(element);
            return element;
        }

        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            if (_content == null)
                return;

            if (builder.Length > 0 && !(this is MarkdownListElement))
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            for (var i = 0; i < _content.Count; i++)
            {
                var element = _content[i];

                if (i > 0)
                {
                    var previous = _content[i - 1];
                    if (element is MarkdownHeader
                        || (element is MarkdownParagraph || element is MarkdownCode) == (previous is MarkdownParagraph || previous is MarkdownCode)
                        || previous is MarkdownHeader
                        || previous is MarkdownTable
                        || previous is MarkdownCode)
                    {
                        builder.AppendLine();
                    }

                    builder.AppendLine();
                }

                element.Resolve(builder);
            }
        }
    }
}

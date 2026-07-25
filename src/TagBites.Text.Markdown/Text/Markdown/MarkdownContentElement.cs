namespace TagBites.Text.Markdown;

public abstract class MarkdownContentElement : MarkdownElement
{
    private IList<MarkdownElement>? _content;

    protected IList<MarkdownElement> Content => _content ??= new List<MarkdownElement>();
    private protected IList<MarkdownElement>? ContentCore => _content;


    public MarkdownSection AddSection(MarkdownText text) => AddCore(new MarkdownSection(text));
    public MarkdownSection AddSection(int level, MarkdownText text) => AddCore(new MarkdownSection(text) { Header = { Level = level } });

    public MarkdownHeader AddHeader(int level, MarkdownText text) => AddCore(new MarkdownHeader(level, text));
    public MarkdownParagraph AddParagraph(MarkdownText text) => AddCore(new MarkdownParagraph(text));
    public MarkdownCode AddCode(string code) => AddCore(new MarkdownCode(null, code));
    public MarkdownCode AddCode(string language, string code) => AddCore(new MarkdownCode(language, code));
    public MarkdownQuote AddQuote() => AddCore(new MarkdownQuote());
    public MarkdownQuote AddQuote(MarkdownText quote) => AddCore(new MarkdownQuote(quote));
    public MarkdownList AddList(bool isOrdered = false) => AddCore(new MarkdownList { IsOrdered = isOrdered });
    public MarkdownTable AddTable() => AddCore(new MarkdownTable());
    public MarkdownThematicBreak AddThematicBreak() => AddCore(new MarkdownThematicBreak());

    protected internal T AddCore<T>(T element) where T : MarkdownElement
    {
        Content.Add(element);
        return element;
    }

    protected internal override void Resolve(MarkdownStringBuilder builder) => ResolveContent(builder, true);

    private protected void ResolveContent(MarkdownStringBuilder builder, bool blankLineBeforeFirst)
    {
        if (_content == null)
            return;

        var first = true;

        foreach (var element in builder.Format.GetVisible(_content))
        {
            if (!first || builder.Length > 0)
            {
                builder.AppendLine();

                if (!first || blankLineBeforeFirst)
                    builder.AppendLine();
            }

            first = false;
            element.Resolve(builder);
        }
    }
}

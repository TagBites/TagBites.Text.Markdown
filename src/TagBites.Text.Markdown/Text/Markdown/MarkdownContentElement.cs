namespace TagBites.Text.Markdown;

/// <summary>
/// An element that holds other elements.
/// </summary>
/// <remarks>
/// An <c>Add</c> method returns the element it created, which moves the chain one level deeper.
/// A <c>With</c> method of <see cref="MarkdownElementExtensions"/> returns the receiver and keeps the chain at the same level.
/// </remarks>
public abstract class MarkdownContentElement : MarkdownElement
{
    private IList<MarkdownElement>? _content;

    /// <summary>
    /// Gets the child elements.
    /// </summary>
    protected IList<MarkdownElement> Content => _content ??= new List<MarkdownElement>();
    private protected IList<MarkdownElement>? ContentCore => _content;


    /// <summary>
    /// Adds a section one level below this one and returns it.
    /// </summary>
    public MarkdownSection AddSection(MarkdownText text) => AddCore(new MarkdownSection(text));
    /// <summary>
    /// Adds a section whose header carries an explicit level and returns it.
    /// </summary>
    /// <remarks>The level does not follow the nesting, so it may also be shallower than this one.</remarks>
    public MarkdownSection AddSection(int level, MarkdownText text) => AddCore(new MarkdownSection(text) { Header = { Level = level } });

    /// <summary>
    /// Adds a header with an explicit level and returns it.
    /// </summary>
    /// <remarks>Use <see cref="AddSection(MarkdownText)"/> for a level that follows the nesting.</remarks>
    public MarkdownHeader AddHeader(int level, MarkdownText text) => AddCore(new MarkdownHeader(level, text));
    /// <summary>
    /// Adds a paragraph and returns it.
    /// </summary>
    public MarkdownParagraph AddParagraph(MarkdownText text) => AddCore(new MarkdownParagraph(text));
    /// <summary>
    /// Adds a code block without a language and returns it.
    /// </summary>
    public MarkdownCode AddCode(string code) => AddCore(new MarkdownCode(null, code));
    /// <summary>
    /// Adds a code block written in <paramref name="language"/> and returns it.
    /// </summary>
    public MarkdownCode AddCode(string language, string code) => AddCore(new MarkdownCode(language, code));
    /// <summary>
    /// Adds a block of raw HTML and returns it.
    /// </summary>
    /// <remarks>The markup reaches the output unchanged, so it must not be built from untrusted text.</remarks>
    public MarkdownHtml AddHtml(string html) => AddCore(new MarkdownHtml(html));
    /// <summary>
    /// Adds a quote that holds only block elements and returns it.
    /// </summary>
    public MarkdownQuote AddQuote() => AddCore(new MarkdownQuote());
    /// <summary>
    /// Adds a quote that starts with <paramref name="quote"/> and returns it.
    /// </summary>
    public MarkdownQuote AddQuote(MarkdownText quote) => AddCore(new MarkdownQuote(quote));
    /// <summary>
    /// Adds an empty list and returns it, numbering the items when <paramref name="isOrdered"/> is <c>true</c>.
    /// </summary>
    public MarkdownList AddList(bool isOrdered = false) => AddCore(new MarkdownList { IsOrdered = isOrdered });
    /// <summary>
    /// Adds an empty table and returns it.
    /// </summary>
    public MarkdownTable AddTable() => AddCore(new MarkdownTable());
    /// <summary>
    /// Adds a thematic break and returns it.
    /// </summary>
    public MarkdownThematicBreak AddThematicBreak() => AddCore(new MarkdownThematicBreak());

    /// <summary>
    /// Adds <paramref name="element"/> to <see cref="Content"/> and returns it.
    /// </summary>
    protected internal T AddCore<T>(T element) where T : MarkdownElement
    {
        Content.Add(element);
        return element;
    }

    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder) => ResolveContent(builder, true);

    private protected void ResolveContent(MarkdownStringBuilder builder, bool blankLineBeforeFirst)
    {
        if (_content == null)
            return;

        var first = true;

        foreach (var element in builder.Format.GetVisible(_content))
        {
            var start = builder.Length;

            if (!first || builder.Length > 0)
            {
                builder.AppendLine();

                if (!first || blankLineBeforeFirst)
                    builder.AppendLine();
            }

            var separated = builder.Length;
            element.Resolve(builder);

            if (builder.Length == separated)
                builder.Truncate(start);
            else
                first = false;
        }
    }
}

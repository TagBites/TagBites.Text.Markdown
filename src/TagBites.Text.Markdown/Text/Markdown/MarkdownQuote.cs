namespace TagBites.Text.Markdown;

/// <summary>
/// A quote, written with the <c>&gt;</c> marker on every line.
/// </summary>
/// <remarks>A quote holds block elements and other quotes, which nest as <c>&gt; &gt;</c>.</remarks>
public class MarkdownQuote(MarkdownText text) : MarkdownContentElement
{
    /// <summary>
    /// Gets or sets the text the quote starts with.
    /// </summary>
    public MarkdownText Text { get; set; } = text;

    /// <summary>
    /// Creates a quote that holds only block elements.
    /// </summary>
    public MarkdownQuote()
        : this(MarkdownText.Empty)
    { }


    /// <inheritdoc />
    protected internal override void Resolve(MarkdownRenderer renderer)
    {
        var marked = !renderer.Format.IsPlainText;

        if (marked)
            renderer.PushPrefix("> ");

        if (!Text.IsEmpty)
            renderer.Append(Text);

        ResolveContent(renderer, true, !Text.IsEmpty);

        if (marked)
            renderer.PopPrefix();
    }
}

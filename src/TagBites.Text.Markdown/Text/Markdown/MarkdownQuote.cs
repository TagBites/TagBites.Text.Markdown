namespace TagBites.Text.Markdown;

/// <summary>
/// A quote, written with the <c>&gt;</c> marker on every line.
/// </summary>
/// <remarks>A quote holds block elements and other quotes, which nest as <c>&gt; &gt;</c>.</remarks>
public class MarkdownQuote(MarkdownText text) : MarkdownContentElement
{
    /// <summary>
    /// Gets the text the quote starts with.
    /// </summary>
    public MarkdownText Text { get; } = text;

    /// <summary>
    /// Creates a quote that holds only block elements.
    /// </summary>
    public MarkdownQuote()
        : this(MarkdownText.Empty)
    { }


    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        // Content is rendered separately, then every line gets the '>' marker
        var inner = new MarkdownStringBuilder(builder.Format);

        // A section inside a quote continues the numbering of the surrounding document
        if (builder.SectionLevel > 0)
            inner.PushSectionLevel(builder.SectionLevel);

        if (!Text.IsEmpty)
            inner.Append(Text);

        base.Resolve(inner);

        var content = inner.ToString();

        if (builder.Format.IsPlainText)
        {
            builder.Append(content);
            return;
        }

        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                builder.AppendLine();

            var line = lines[i].TrimEnd('\r');

            if (line.Length == 0)
                builder.Append(">");
            else
            {
                builder.Append("> ");
                builder.Append(line);
            }
        }
    }
}

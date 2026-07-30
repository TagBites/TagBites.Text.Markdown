namespace TagBites.Text.Markdown;

/// <summary>
/// A header, written with the <c>#</c> marker.
/// </summary>
/// <remarks>
/// Markdown has no header below level six.
/// A deeper one becomes bold text followed by a hard line break, which reads as a header in every renderer.
/// </remarks>
public class MarkdownHeader : MarkdownElement
{
    internal const int MinimumLevel = 1;
    internal const int MaximumLevel = 6;

    /// <summary>
    /// Gets or sets the level of the header, or <c>null</c> when it follows the section nesting.
    /// </summary>
    public int? Level
    {
        get;
        set
        {
            if (value != null && value is < MinimumLevel or > MaximumLevel)
                throw new ArgumentOutOfRangeException(nameof(value));

            field = value;
        }
    }
    /// <summary>
    /// Gets or sets the text of the header.
    /// </summary>
    public MarkdownText Text { get; set; }

    /// <summary>
    /// Gets or sets the explicit anchor of the header.
    /// </summary>
    /// <remarks>
    /// <see cref="MarkdownFormat.HeaderAnchorStyle"/> decides how the anchor is written.
    /// The property drops a leading <c>#</c>.
    /// </remarks>
    public string? CustomId
    {
        get;
        set
        {
            ValidateCustomId(value);
            field = value;
        }
    }

    /// <summary>
    /// Creates a header whose level follows the section nesting.
    /// </summary>
    public MarkdownHeader(MarkdownText text) => Text = text;
    /// <summary>
    /// Creates a header with an explicit level, between one and six.
    /// </summary>
    public MarkdownHeader(int level, MarkdownText text)
    {
        Level = level;
        Text = text;
    }


    /// <inheritdoc cref="CustomId"/>
    public MarkdownHeader SetCustomId(string customId)
    {
        CustomId = customId;
        return this;
    }

    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        Write(builder, Level ?? builder.SectionLevel + 1, Text, CustomId);
    }

    internal static void Write(MarkdownStringBuilder builder, int level, MarkdownText text, string? customId)
    {
        if (builder.Format.IsPlainText)
        {
            builder.Append(text);
            return;
        }

        var anchor = GetAnchor(customId);
        var style = builder.Format.HeaderAnchorStyle;

        if (level > MaximumLevel)
        {
            WriteAnchor(builder, anchor, style, MarkdownHeaderAnchorStyle.HtmlAnchor);
            builder.Append(MarkdownText.Bold(text));
            WriteAnchor(builder, anchor, style, MarkdownHeaderAnchorStyle.Attribute);
            return;
        }

        builder.Append('#', level);
        builder.Append(' ');

        WriteAnchor(builder, anchor, style, MarkdownHeaderAnchorStyle.HtmlAnchor);
        builder.Append(text);
        WriteAnchor(builder, anchor, style, MarkdownHeaderAnchorStyle.Attribute);
    }
    internal static void ValidateCustomId(string? value)
    {
        if (value == null)
            return;

        foreach (var c in value)
            if (c is '{' or '}' or '"' or '<' or '>' || char.IsWhiteSpace(c))
                throw new ArgumentException("Custom id can not contain a brace, a quote, an angle bracket or white space.", nameof(value));
    }

    private static void WriteAnchor(MarkdownStringBuilder builder, string? anchor, MarkdownHeaderAnchorStyle style, MarkdownHeaderAnchorStyle expected)
    {
        if (anchor == null || style != expected)
            return;

        if (style == MarkdownHeaderAnchorStyle.HtmlAnchor)
        {
            builder.Append("<a id=\"");
            builder.Append(anchor);
            builder.Append("\"></a> ");
        }
        else
        {
            builder.Append("{#");
            builder.Append(anchor);
            builder.Append('}');
        }
    }
    private static string? GetAnchor(string? customId)
    {
        if (customId == null || customId.Length == 0)
            return null;

        return customId[0] == '#' ? customId.Substring(1) : customId;
    }
}

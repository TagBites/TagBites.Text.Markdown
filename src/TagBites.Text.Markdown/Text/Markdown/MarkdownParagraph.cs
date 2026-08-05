namespace TagBites.Text.Markdown;

/// <summary>
/// A block of text separated from its neighbours by a blank line.
/// </summary>
public class MarkdownParagraph(MarkdownText text) : MarkdownElement
{
    /// <summary>
    /// Gets or sets the content of the paragraph.
    /// </summary>
    public MarkdownText Text { get; set; } = text;


    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        builder.Append(Text);
    }
}

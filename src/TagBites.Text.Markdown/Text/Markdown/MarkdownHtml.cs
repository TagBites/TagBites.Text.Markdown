namespace TagBites.Text.Markdown;

/// <summary>
/// A block of raw HTML, for markup Markdown has no syntax for.
/// </summary>
/// <remarks>The content reaches the output unchanged, so it must not be built from untrusted text.</remarks>
public class MarkdownHtml(string html) : MarkdownElement
{
    /// <summary>
    /// Gets or sets the markup written to the output.
    /// </summary>
    public string Html { get; set; } = html;


    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        if (!string.IsNullOrEmpty(Html))
            builder.Append(Html);
    }
}

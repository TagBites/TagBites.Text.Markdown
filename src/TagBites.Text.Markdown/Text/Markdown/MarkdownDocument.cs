namespace TagBites.Text.Markdown;

/// <summary>
/// The root of a document.
/// </summary>
public class MarkdownDocument : MarkdownContentElement
{
    /// <summary>
    /// Gets the child elements.
    /// </summary>
    public new IList<MarkdownElement> Content => base.Content;

    /// <summary>
    /// Gets or sets the metadata written above the content as a YAML front matter block.
    /// </summary>
    /// <remarks>
    /// Default: <c>null</c>, which writes no block. Plain text output leaves the block out.
    /// </remarks>
    public MarkdownFrontMatter? FrontMatter { get; set; }


    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        // Front matter carries metadata about the document, which is not part of its text
        if (FrontMatter != null && !builder.Format.IsPlainText)
            FrontMatter.Resolve(builder);

        ResolveContent(builder, true);
    }
}

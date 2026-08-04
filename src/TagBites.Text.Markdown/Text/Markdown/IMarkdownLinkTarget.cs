namespace TagBites.Text.Markdown;

/// <summary>
/// An element a link points at, which is a header or the section that owns one.
/// </summary>
/// <remarks>Pass an instance to <see cref="MarkdownText.Link(IMarkdownLinkTarget)"/> instead of writing the anchor by hand.</remarks>
public interface IMarkdownLinkTarget
{
    /// <summary>
    /// Gets the text a link uses when the caller supplies none.
    /// </summary>
    MarkdownText Text { get; }
    /// <summary>
    /// Gets the identifier a link addresses, or <c>null</c> when the target carries none.
    /// </summary>
    string? AnchorId { get; }
}

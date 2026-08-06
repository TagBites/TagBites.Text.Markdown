namespace TagBites.Text.Markdown;

/// <summary>
/// A header together with the content that belongs under it.
/// </summary>
/// <remarks>
/// The level of the header follows the nesting, so moving a section moves its whole subtree.
/// Set <see cref="MarkdownHeader.Level"/> for a level that does not follow the nesting.
/// </remarks>
public class MarkdownSection(MarkdownText text) : MarkdownContentElement, IMarkdownLinkTarget
{
    /// <summary>
    /// Gets the header of the section.
    /// </summary>
    public MarkdownHeader Header { get; } = new(text);

    MarkdownText IMarkdownLinkTarget.Text => Header.Text;
    string? IMarkdownLinkTarget.AnchorId => Header.AnchorId;


    /// <inheritdoc cref="MarkdownHeader.CustomId"/>
    public MarkdownSection SetCustomId(string customId)
    {
        Header.CustomId = customId;
        return this;
    }

    /// <inheritdoc />
    protected internal override void Resolve(MarkdownRenderer renderer)
    {
        var level = Header.Level ?? renderer.SectionLevel + 1;

        MarkdownHeader.Write(renderer, level, Header.Text, Header.CustomId);

        // A header below level six is bold text, which the content follows on the next line
        var blankLineBeforeContent = renderer.Format.IsPlainText || level <= MarkdownHeader.MaximumLevel;
        if (!blankLineBeforeContent)
            renderer.Append("  ");

        renderer.PushSectionLevel(level);
        ResolveContent(renderer, blankLineBeforeContent);
        renderer.PopSectionLevel();
    }
}

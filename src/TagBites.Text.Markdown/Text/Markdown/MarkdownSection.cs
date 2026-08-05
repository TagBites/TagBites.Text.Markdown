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
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        var level = Header.Level ?? builder.SectionLevel + 1;

        MarkdownHeader.Write(builder, level, Header.Text, Header.CustomId);

        // A header below level six is bold text, which the content follows on the next line
        var blankLineBeforeContent = builder.Format.IsPlainText || level <= MarkdownHeader.MaximumLevel;
        if (!blankLineBeforeContent)
            builder.Append("  ");

        builder.PushSectionLevel(level);
        ResolveContent(builder, blankLineBeforeContent);
        builder.PopSectionLevel();
    }
}

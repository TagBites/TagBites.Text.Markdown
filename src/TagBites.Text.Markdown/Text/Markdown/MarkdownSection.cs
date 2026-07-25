using System.ComponentModel;

namespace TagBites.Text.Markdown;

public class MarkdownSection(MarkdownText text) : MarkdownContentElement
{
    public MarkdownHeader Header { get; } = new(text);


    public MarkdownSection SetCustomId(string customId)
    {
        Header.CustomId = customId;
        return this;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new MarkdownHeader AddHeader(int level, MarkdownText text)
    {
        throw new NotSupportedException("A section writes its own header. Nest a section instead of adding a header to it.");
    }

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

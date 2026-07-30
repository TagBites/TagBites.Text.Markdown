namespace TagBites.Text.Markdown;

/// <summary>
/// A horizontal rule between two blocks, written as <c>---</c>.
/// </summary>
/// <remarks>Plain text output writes nothing.</remarks>
public class MarkdownThematicBreak : MarkdownElement
{
    /// <inheritdoc />
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        if (!builder.Format.IsPlainText)
            builder.Append("---");
    }
}

namespace TagBites.Text.Markdown;

/// <summary>
/// A horizontal rule between two blocks, written as <c>---</c>.
/// </summary>
/// <remarks>Plain text output writes nothing.</remarks>
public class MarkdownThematicBreak : MarkdownElement
{
    /// <inheritdoc />
    protected internal override void Resolve(MarkdownRenderer renderer)
    {
        if (!renderer.Format.IsPlainText)
            renderer.Append("---");
    }
}

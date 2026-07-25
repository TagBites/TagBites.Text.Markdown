namespace TagBites.Text.Markdown;

public class MarkdownThematicBreak : MarkdownElement
{
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        if (!builder.Format.IsPlainText)
            builder.Append("---");
    }
}

namespace TagBites.Text.Markdown;

public class MarkdownParagraph(MarkdownText text) : MarkdownElement
{
    public MarkdownText Text { get; } = text;


    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        builder.Append(Text);
    }
}

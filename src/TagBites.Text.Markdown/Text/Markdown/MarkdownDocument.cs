namespace TagBites.Text.Markdown;

public class MarkdownDocument : MarkdownContentElement
{
    public new IList<MarkdownElement> Content => base.Content;

    public MarkdownFrontMatter? FrontMatter { get; set; }


    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        // Front matter carries metadata about the document, which is not part of its text
        if (FrontMatter != null && !builder.Format.IsPlainText)
            FrontMatter.Resolve(builder);

        ResolveContent(builder, true);
    }
}

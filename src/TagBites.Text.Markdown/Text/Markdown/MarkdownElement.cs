namespace TagBites.Text.Markdown;

public abstract class MarkdownElement
{
    protected internal abstract void Resolve(MarkdownStringBuilder builder);

    public override string ToString() => ToString(MarkdownFormat.Default);
    public string ToString(MarkdownFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        var builder = new MarkdownStringBuilder(format);
        Resolve(builder);
        return builder.ToString();
    }
}

namespace TagBites.Text.Markdown;

public class MarkdownQuote(MarkdownText text) : MarkdownContentElement
{
    public MarkdownText Text { get; } = text;

    public MarkdownQuote()
        : this(MarkdownText.Empty)
    { }


    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        // Content is rendered separately, then every line gets the '>' marker
        var inner = new MarkdownStringBuilder(builder.Format);

        // A section inside a quote continues the numbering of the surrounding document
        if (builder.SectionLevel > 0)
            inner.PushSectionLevel(builder.SectionLevel);

        if (!Text.IsEmpty)
            inner.Append(Text);

        base.Resolve(inner);

        var content = inner.ToString();

        if (builder.Format.IsPlainText)
        {
            builder.Append(content);
            return;
        }

        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                builder.AppendLine();

            var line = lines[i].TrimEnd('\r');

            if (line.Length == 0)
                builder.Append(">");
            else
            {
                builder.Append("> ");
                builder.Append(line);
            }
        }
    }
}

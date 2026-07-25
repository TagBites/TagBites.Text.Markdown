namespace TagBites.Text.Markdown;

public class MarkdownCode(string? language, string code) : MarkdownElement
{
    private const int MinimumFenceLength = 3;

    public string? Language { get; } = language;
    public string Code { get; } = code;


    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        if (builder.Format.IsPlainText)
        {
            builder.Append(Code);
            return;
        }

        // The fence has to outlast the longest run of backticks in the code
        var length = Math.Max(MinimumFenceLength, MarkdownEscaper.GetLongestRun(Code, '`') + 1);

        builder.Append('`', length);
        {
            if (Language is { Length: > 0 })
                builder.Append(Language);

            builder.AppendLine();
            builder.Append(Code);
            builder.AppendLine();
        }
        builder.Append('`', length);
    }
}

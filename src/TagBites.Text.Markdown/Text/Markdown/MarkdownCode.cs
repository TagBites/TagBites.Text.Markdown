namespace TagBites.Text.Markdown;

/// <summary>
/// A fenced block of code, with an optional language.
/// </summary>
/// <remarks>
/// The block writes its content unchanged, because escaping would alter the code.
/// The fence outlasts the longest run of backticks inside, so code that holds a fence cannot end the block.
/// </remarks>
public class MarkdownCode(string? language, string code) : MarkdownElement
{
    private const int MinimumFenceLength = 3;

    /// <summary>
    /// Gets the language written after the opening fence, or <c>null</c> when the block has none.
    /// </summary>
    public string? Language { get; } = language;
    /// <summary>
    /// Gets the content of the block.
    /// </summary>
    public string Code { get; } = code;


    /// <inheritdoc />
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

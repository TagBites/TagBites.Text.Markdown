namespace TagBites.Text.Markdown;

/// <summary>
/// Carries a piece of content together with its Markdown form.
/// </summary>
/// <remarks>
/// A <see cref="string"/> converts implicitly and is escaped, so untrusted text cannot introduce markup.
/// Use <see cref="Raw(string)"/> for content that is already Markdown.
/// Escaping is minimal, because a character keeps its plain form where it carries no syntax.
/// </remarks>
public readonly struct MarkdownText : IEquatable<MarkdownText>
{
    /// <summary>
    /// Gets an instance without content.
    /// </summary>
    public static MarkdownText Empty => default;
    /// <summary>
    /// Gets a hard line break, written as two spaces and a new line.
    /// </summary>
    public static MarkdownText LineBreak => new("  \n", "\n");

    private readonly string? _markdown;
    private readonly string? _text;

    /// <summary>
    /// Gets the Markdown form, with syntax characters escaped unless the instance is raw.
    /// </summary>
    public string Markdown => _markdown ?? string.Empty;
    /// <summary>
    /// Gets the content without Markdown syntax, used by the clean text mode.
    /// </summary>
    public string Text => _text ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the instance carries no content.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_markdown) && string.IsNullOrEmpty(_text);

    private MarkdownText(string markdown, string text)
    {
        _markdown = markdown;
        _text = text;
    }


    /// <summary>
    /// Returns a value indicating whether both forms of <paramref name="other"/> match this instance.
    /// </summary>
    public bool Equals(MarkdownText other) => Markdown == other.Markdown && Text == other.Text;
    /// <inheritdoc cref="Equals(MarkdownText)"/>
    public override bool Equals(object? obj) => obj is MarkdownText other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (Markdown.GetHashCode() * 397) ^ Text.GetHashCode();
        }
    }

    /// <remarks>Returns <see cref="Markdown"/>.</remarks>
    public override string ToString() => Markdown;

    /// <summary>
    /// Creates an instance from plain text, escaping the characters that carry syntax.
    /// </summary>
    public static MarkdownText FromText(string text)
    {
        return text == null
            ? Empty
            : new MarkdownText(MarkdownEscaper.Escape(text), text);
    }
    /// <summary>
    /// Creates an instance from content that is already Markdown and must not be escaped.
    /// </summary>
    public static MarkdownText Raw(string markdown) => markdown == null ? Empty : new MarkdownText(markdown, markdown);
    /// <summary>
    /// Creates an instance from content that is already Markdown, with the text the clean text mode returns.
    /// </summary>
    public static MarkdownText Raw(string markdown, string text) => new(markdown ?? string.Empty, text ?? string.Empty);

    /// <summary>
    /// Wraps the content in bold markers.
    /// </summary>
    public static MarkdownText Bold(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("**" + text.Markdown + "**", text.Text);
    /// <summary>
    /// Wraps the content in italic markers.
    /// </summary>
    public static MarkdownText Italic(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("_" + text.Markdown + "_", text.Text);
    /// <summary>
    /// Wraps the content in strikethrough markers, which the GitHub Flavored Markdown extension defines.
    /// </summary>
    public static MarkdownText Strikethrough(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("~~" + text.Markdown + "~~", text.Text);
    /// <summary>
    /// Wraps the content in a code span.
    /// </summary>
    /// <remarks>
    /// The delimiter outlasts the longest run of backticks in the content.
    /// A padding space keeps a backtick or a space at the edge inside the span.
    /// </remarks>
    public static MarkdownText Code(string code)
    {
        if (string.IsNullOrEmpty(code))
            return Empty;

        var delimiter = new string('`', MarkdownEscaper.GetLongestRun(code, '`') + 1);
        var padding = NeedsPadding(code) ? " " : string.Empty;

        return new MarkdownText(delimiter + padding + code + padding + delimiter, code);
    }
    /// <summary>
    /// Builds a link to <paramref name="address"/>.
    /// </summary>
    /// <remarks>The display text is escaped, the address is not.</remarks>
    public static MarkdownText Link(MarkdownText name, string address)
    {
        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("Address can not be null or empty.", nameof(address));

        return new MarkdownText("[" + name.Markdown + "](" + address + ")", name.Text);
    }
    /// <summary>
    /// Builds an image held at <paramref name="address"/>.
    /// </summary>
    /// <inheritdoc cref="Link"/>
    public static MarkdownText Image(MarkdownText name, string address)
    {
        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("Address can not be null or empty.", nameof(address));

        return new MarkdownText("![" + name.Markdown + "](" + address + ")", name.Text);
    }

    private static bool NeedsPadding(string code)
    {
        var first = code[0];
        var last = code[code.Length - 1];

        return first == '`'
               || last == '`'
               || (first == ' ' && last == ' ' && code.Trim().Length > 0);
    }

    /// <summary>
    /// Converts plain text, escaping the characters that carry syntax.
    /// </summary>
    public static implicit operator MarkdownText(string text) => FromText(text);
    /// <summary>
    /// Joins two instances, keeping the escaping decision each of them was built with.
    /// </summary>
    public static MarkdownText operator +(MarkdownText left, MarkdownText right) => new(left.Markdown + right.Markdown, left.Text + right.Text);

    /// <inheritdoc cref="Equals(MarkdownText)"/>
    public static bool operator ==(MarkdownText left, MarkdownText right) => left.Equals(right);
    /// <inheritdoc cref="Equals(MarkdownText)"/>
    public static bool operator !=(MarkdownText left, MarkdownText right) => !left.Equals(right);
}

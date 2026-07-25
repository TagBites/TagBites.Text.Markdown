namespace TagBites.Text.Markdown;

public readonly struct MarkdownText : IEquatable<MarkdownText>
{
    public static MarkdownText Empty => default;
    public static MarkdownText LineBreak => new("  \n", "\n");

    private readonly string? _markdown;
    private readonly string? _text;

    public string Markdown => _markdown ?? string.Empty;
    public string Text => _text ?? string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(_markdown) && string.IsNullOrEmpty(_text);

    private MarkdownText(string markdown, string text)
    {
        _markdown = markdown;
        _text = text;
    }


    public bool Equals(MarkdownText other) => Markdown == other.Markdown && Text == other.Text;
    public override bool Equals(object? obj) => obj is MarkdownText other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            return (Markdown.GetHashCode() * 397) ^ Text.GetHashCode();
        }
    }

    public override string ToString() => Markdown;

    public static MarkdownText FromText(string text)
    {
        return text == null
            ? Empty
            : new MarkdownText(MarkdownEscaper.Escape(text), text);
    }
    public static MarkdownText Raw(string markdown) => markdown == null ? Empty : new MarkdownText(markdown, markdown);
    public static MarkdownText Raw(string markdown, string text) => new(markdown ?? string.Empty, text ?? string.Empty);

    public static MarkdownText Bold(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("**" + text.Markdown + "**", text.Text);
    public static MarkdownText Italic(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("_" + text.Markdown + "_", text.Text);
    public static MarkdownText Strikethrough(MarkdownText text) => text.IsEmpty ? Empty : new MarkdownText("~~" + text.Markdown + "~~", text.Text);
    public static MarkdownText Code(string code)
    {
        if (string.IsNullOrEmpty(code))
            return Empty;

        var delimiter = new string('`', MarkdownEscaper.GetLongestRun(code, '`') + 1);
        var padding = NeedsPadding(code) ? " " : string.Empty;

        return new MarkdownText(delimiter + padding + code + padding + delimiter, code);
    }
    public static MarkdownText Link(MarkdownText name, string address)
    {
        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("Address can not be null or empty.", nameof(address));

        return new MarkdownText("[" + name.Markdown + "](" + address + ")", name.Text);
    }
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

    public static implicit operator MarkdownText(string text) => FromText(text);
    public static MarkdownText operator +(MarkdownText left, MarkdownText right) => new(left.Markdown + right.Markdown, left.Text + right.Text);

    public static bool operator ==(MarkdownText left, MarkdownText right) => left.Equals(right);
    public static bool operator !=(MarkdownText left, MarkdownText right) => !left.Equals(right);
}

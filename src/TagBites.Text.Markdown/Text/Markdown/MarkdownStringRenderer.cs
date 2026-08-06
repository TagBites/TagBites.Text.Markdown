using System.Text;

namespace TagBites.Text.Markdown;

internal class MarkdownStringRenderer : MarkdownRenderer
{
    private readonly StringBuilder _text;

    public override int Length => _text.Length;

    public MarkdownStringRenderer(MarkdownFormat format)
        : this(new StringBuilder(), format)
    { }
    public MarkdownStringRenderer(StringBuilder text, MarkdownFormat format)
        : base(format)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }


    protected override void Write(char value) => _text.Append(value);
    protected override void Write(string value) => _text.Append(value);
    protected override char TruncateCore(int length)
    {
        _text.Length = length;
        return length > 0 ? _text[length - 1] : '\n';
    }

    public override string ToString() => _text.ToString();
}

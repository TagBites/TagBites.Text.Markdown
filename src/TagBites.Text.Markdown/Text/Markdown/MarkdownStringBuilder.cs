using System.Text;

namespace TagBites.Text.Markdown;

public class MarkdownStringBuilder
{
    private readonly Stack<int> _indents = new();
    private readonly Stack<int> _sectionLevels = new();

    private StringBuilder StringBuilder { get; }
    public int Indent { get; private set; }
    public int Length => StringBuilder.Length;

    public MarkdownFormat Format { get; }

    public int SectionLevel { get; private set; }

    public MarkdownStringBuilder(MarkdownFormat format)
        : this(new StringBuilder(), format)
    { }
    public MarkdownStringBuilder(StringBuilder stringBuilder, MarkdownFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        // Writing freezes the format, so it cannot change while a document is produced
        format.MakeReadOnly();

        StringBuilder = stringBuilder;
        Format = format;
    }


    public void PushIndent(int columns)
    {
        if (columns < 0)
            throw new ArgumentOutOfRangeException(nameof(columns));

        _indents.Push(Indent);
        Indent += columns;
    }
    public void PopIndent() => Indent = _indents.Pop();

    public void PushSectionLevel(int level)
    {
        if (level < 0)
            throw new ArgumentOutOfRangeException(nameof(level));

        _sectionLevels.Push(SectionLevel);
        SectionLevel = level;
    }
    public void PopSectionLevel() => SectionLevel = _sectionLevels.Pop();

    public void Append(char value)
    {
        if (value == '\n')
            AppendLine();
        else
        {
            AppendIndent();
            StringBuilder.Append(value);
        }
    }
    public void Append(string value)
    {
        var lines = value.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                AppendLine();

            AppendIndent();
            StringBuilder.Append(lines[i]);
        }
    }
    public void Append(char value, int count)
    {
        if (value == '\n')
        {
            for (var i = 0; i < count; i++)
                AppendLine();
        }
        else
        {
            AppendIndent();

            for (var i = 0; i < count; i++)
                StringBuilder.Append(value);
        }
    }
    public void Append(MarkdownText value) => Append(Format.IsPlainText ? value.Text : value.Markdown);
    public void AppendSpaces(int count) => Append(' ', count);
    public void AppendLine()
    {
        StringBuilder.Append("\n");
    }
    private void AppendIndent()
    {
        if (StringBuilder.Length == 0 || StringBuilder[StringBuilder.Length - 1] == '\n')
            for (var i = 0; i < Indent; i++)
                StringBuilder.Append(' ');
    }

    public override string ToString() => StringBuilder.ToString();
}

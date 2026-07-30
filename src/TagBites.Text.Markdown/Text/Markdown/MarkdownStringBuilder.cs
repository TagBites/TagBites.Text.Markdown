using System.Text;

namespace TagBites.Text.Markdown;

/// <summary>
/// The output an element writes itself into.
/// </summary>
/// <remarks>
/// Keeps the indent and the section level that the surrounding elements established.
/// An element therefore does not need to know where it sits in the document.
/// </remarks>
public class MarkdownStringBuilder
{
    private readonly Stack<int> _indents = new();
    private readonly Stack<int> _sectionLevels = new();

    private StringBuilder StringBuilder { get; }
    /// <summary>
    /// Gets the number of spaces every new line is indented by.
    /// </summary>
    public int Indent { get; private set; }
    /// <summary>
    /// Gets the number of characters written so far.
    /// </summary>
    public int Length => StringBuilder.Length;

    /// <summary>
    /// Gets the format the document is written with.
    /// </summary>
    public MarkdownFormat Format { get; }

    /// <summary>
    /// Gets the level of the section being written, where zero means no section.
    /// </summary>
    public int SectionLevel { get; private set; }

    /// <summary>
    /// Creates a builder that writes into a new <see cref="System.Text.StringBuilder"/>.
    /// </summary>
    public MarkdownStringBuilder(MarkdownFormat format)
        : this(new StringBuilder(), format)
    { }
    /// <summary>
    /// Creates a builder that appends to <paramref name="stringBuilder"/>.
    /// </summary>
    /// <remarks>The constructor makes <paramref name="format"/> read-only.</remarks>
    public MarkdownStringBuilder(StringBuilder stringBuilder, MarkdownFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        // Writing freezes the format, so it cannot change while a document is produced
        format.MakeReadOnly();

        StringBuilder = stringBuilder;
        Format = format;
    }


    /// <summary>
    /// Indents the following lines by <paramref name="columns"/> more spaces.
    /// </summary>
    /// <remarks>
    /// A block inside a list item lines up with the text of the item, so the caller passes the width
    /// of the marker it has just written.
    /// </remarks>
    public void PushIndent(int columns)
    {
        if (columns < 0)
            throw new ArgumentOutOfRangeException(nameof(columns));

        _indents.Push(Indent);
        Indent += columns;
    }
    /// <summary>
    /// Restores the indent that was in place before the last <see cref="PushIndent"/>.
    /// </summary>
    public void PopIndent() => Indent = _indents.Pop();

    /// <summary>
    /// Sets the level the sections written next are nested under.
    /// </summary>
    public void PushSectionLevel(int level)
    {
        if (level < 0)
            throw new ArgumentOutOfRangeException(nameof(level));

        _sectionLevels.Push(SectionLevel);
        SectionLevel = level;
    }
    /// <summary>
    /// Restores the level that was in place before the last <see cref="PushSectionLevel"/>.
    /// </summary>
    public void PopSectionLevel() => SectionLevel = _sectionLevels.Pop();

    /// <summary>
    /// Appends a single character, applying the indent at the start of a line.
    /// </summary>
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
    /// <inheritdoc cref="Append(char)"/>
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
    /// <summary>
    /// Appends <paramref name="value"/> <paramref name="count"/> times.
    /// </summary>
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
    /// <summary>
    /// Appends the form of <paramref name="value"/> that matches <see cref="Format"/>.
    /// </summary>
    public void Append(MarkdownText value) => Append(Format.IsPlainText ? value.Text : value.Markdown);
    /// <summary>
    /// Appends <paramref name="count"/> spaces.
    /// </summary>
    public void AppendSpaces(int count) => Append(' ', count);
    /// <summary>
    /// Ends the current line.
    /// </summary>
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

    /// <summary>
    /// Returns everything written so far.
    /// </summary>
    public override string ToString() => StringBuilder.ToString();
}

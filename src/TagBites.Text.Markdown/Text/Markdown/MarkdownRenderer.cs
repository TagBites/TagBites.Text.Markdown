using System.Text;

namespace TagBites.Text.Markdown;

/// <summary>
/// The output an element writes itself into.
/// </summary>
public abstract class MarkdownRenderer
{
    private readonly Stack<int> _prefixes = new();
    private readonly Stack<int> _sectionLevels = new();
    private readonly StringBuilder _prefix = new();
    private bool _atLineStart = true;

    /// <summary>
    /// Gets the text every new line starts with.
    /// </summary>
    public string Prefix => _prefix.ToString();
    /// <summary>
    /// Gets the number of characters written so far.
    /// </summary>
    public abstract int Length { get; }

    /// <summary>
    /// Gets the format the document is written with.
    /// </summary>
    public MarkdownFormat Format { get; }

    /// <summary>
    /// Gets the level of the section being written, where zero means no section.
    /// </summary>
    public int SectionLevel { get; private set; }

    /// <summary>
    /// Creates a renderer that writes with the given format, which the constructor makes read-only.
    /// </summary>
    protected MarkdownRenderer(MarkdownFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        // Writing freezes the format, so it cannot change while a document is produced
        format.MakeReadOnly();

        Format = format;
    }


    /// <summary>
    /// Starts every following line with <paramref name="prefix"/> on top of the current one.
    /// </summary>
    /// <remarks>An empty line receives the prefix without its trailing spaces, so a quote keeps its marker.</remarks>
    public void PushPrefix(string prefix)
    {
        if (prefix == null)
            throw new ArgumentNullException(nameof(prefix));

        _prefixes.Push(prefix.Length);
        _prefix.Append(prefix);
    }
    /// <summary>
    /// Removes the prefix added by the last <see cref="PushPrefix"/>.
    /// </summary>
    public void PopPrefix() => _prefix.Length -= _prefixes.Pop();

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
    /// Appends a single character, writing the prefix at the start of a line.
    /// </summary>
    public void Append(char value)
    {
        if (value == '\n')
            AppendLine();
        else
        {
            WritePrefix(false);
            Write(value);
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

            WritePrefix(false);
            Write(lines[i]);
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
            WritePrefix(false);

            for (var i = 0; i < count; i++)
                Write(value);
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
        // A line that stays empty keeps the prefix without its trailing spaces, so '> ' becomes '>'
        WritePrefix(true);

        Write('\n');
        _atLineStart = true;

        OnLineWritten();
    }

    /// <summary>
    /// Sends everything the renderer still holds to its destination.
    /// </summary>
    public virtual void Flush() { }

    /// <summary>
    /// Writes to the destination, without touching the prefix.
    /// </summary>
    protected abstract void Write(char value);
    /// <inheritdoc cref="Write(char)"/>
    protected abstract void Write(string value);
    /// <summary>
    /// Drops everything past <paramref name="length"/>.
    /// </summary>
    /// <returns>The character the output now ends with, or <c>'\n'</c> when nothing is left.</returns>
    protected abstract char TruncateCore(int length);
    /// <summary>
    /// Runs after every line has been written.
    /// </summary>
    protected virtual void OnLineWritten() { }

    private void WritePrefix(bool trimEnd)
    {
        if (!_atLineStart)
            return;

        _atLineStart = false;

        var length = _prefix.Length;

        if (trimEnd)
            while (length > 0 && _prefix[length - 1] == ' ')
                length--;

        for (var i = 0; i < length; i++)
            Write(_prefix[i]);
    }

    internal void Truncate(int length) => _atLineStart = TruncateCore(length) == '\n';
}

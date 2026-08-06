using System.IO;
using System.Text;

namespace TagBites.Text.Markdown;

/// <summary>
/// The base type of every node of a document.
/// </summary>
public abstract class MarkdownElement
{
    /// <summary>
    /// Writes the element into <paramref name="renderer"/>.
    /// </summary>
    protected internal abstract void Resolve(MarkdownRenderer renderer);

    /// <summary>
    /// Returns the element written with <see cref="MarkdownFormat.Default"/>.
    /// </summary>
    public override string ToString() => ToString(MarkdownFormat.Default);
    /// <summary>
    /// Returns the element written with the given format.
    /// </summary>
    /// <remarks>Writing makes <paramref name="format"/> read-only.</remarks>
    public string ToString(MarkdownFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        var renderer = new MarkdownStringRenderer(format);
        Resolve(renderer);
        return renderer.ToString();
    }

    /// <summary>
    /// Writes the element to <paramref name="stream"/> as UTF-8 without a byte order mark.
    /// </summary>
    /// <remarks>The content is sent as it is produced, and the stream is left open.</remarks>
    public void WriteTo(Stream stream) => WriteTo(stream, MarkdownFormat.Default);
    /// <inheritdoc cref="WriteTo(Stream)"/>
    public void WriteTo(Stream stream, MarkdownFormat format)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
        WriteTo(writer, format);
    }
    /// <summary>
    /// Writes the element to <paramref name="writer"/>.
    /// </summary>
    /// <inheritdoc cref="WriteTo(Stream)"/>
    public void WriteTo(TextWriter writer) => WriteTo(writer, MarkdownFormat.Default);
    /// <inheritdoc cref="WriteTo(TextWriter)"/>
    public void WriteTo(TextWriter writer, MarkdownFormat format)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        var renderer = new MarkdownStreamRenderer(writer, format);
        Resolve(renderer);
        renderer.Flush();
    }
}

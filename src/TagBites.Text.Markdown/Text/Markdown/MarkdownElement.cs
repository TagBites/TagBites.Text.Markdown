namespace TagBites.Text.Markdown;

/// <summary>
/// The base type of every node of a document.
/// </summary>
public abstract class MarkdownElement
{
    /// <summary>
    /// Writes the element into <paramref name="builder"/>.
    /// </summary>
    protected internal abstract void Resolve(MarkdownStringBuilder builder);

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

        var builder = new MarkdownStringBuilder(format);
        Resolve(builder);
        return builder.ToString();
    }
}

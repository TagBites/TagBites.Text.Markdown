using TagBites.Collections;

namespace TagBites.Text.Markdown;

/// <summary>
/// Controls how a document is written out.
/// </summary>
/// <remarks>
/// Holds the choices that depend on the target renderer, not on the content of the document.
/// The instance becomes read-only the first time it is used for writing, so a format shared between threads cannot change.
/// </remarks>
public sealed class MarkdownFormat
{
    /// <summary>
    /// Gets the format used when none is given.
    /// </summary>
    public static MarkdownFormat Default { get; } = CreateReadOnly(new MarkdownFormat());
    /// <summary>
    /// Gets a format that renders plain text instead of Markdown.
    /// </summary>
    public static MarkdownFormat PlainText { get; } = CreateReadOnly(new MarkdownFormat { Output = MarkdownOutputKind.PlainText });

    /// <summary>
    /// Gets a value indicating whether the instance rejects further changes.
    /// </summary>
    public bool IsReadOnly { get; private set; }

    /// <summary>
    /// Gets or sets the syntax the document is written in.
    /// </summary>
    /// <remarks>Default: <see cref="MarkdownOutputKind.Markdown"/>.</remarks>
    public MarkdownOutputKind Output
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    }
    /// <summary>
    /// Gets or sets the element types left out of the output, together with their content.
    /// </summary>
    /// <remarks>
    /// A type also covers the types that derive from it.
    /// Inline content belongs to the text of an element, so the format never leaves it out.
    /// Ignoring <see cref="MarkdownCode"/> drops code blocks and keeps a code span inside a sentence.
    /// </remarks>
    public ISet<Type> IgnoredElementTypes
    {
        get => field ??= new HashSet<Type>();
        set
        {
            ThrowIfReadOnly();
            field = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
    /// <summary>
    /// Gets or sets the form used to write an explicit header anchor.
    /// </summary>
    /// <remarks>Default: <see cref="MarkdownHeaderAnchorStyle.HtmlAnchor"/>, which needs no renderer extension.</remarks>
    public MarkdownHeaderAnchorStyle HeaderAnchorStyle
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether a blank line separates the items of a loose list.
    /// </summary>
    /// <remarks>
    /// A list is loose when an item holds a block other than a nested item, such as a paragraph or a
    /// table. Default: <c>true</c>, which is the form a renderer writes. Plain text output is not affected.
    /// </remarks>
    public bool SeparateLooseListItems
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    } = true;

    /// <summary>
    /// Gets a value indicating whether <see cref="Output"/> is <see cref="MarkdownOutputKind.PlainText"/>.
    /// </summary>
    public bool IsPlainText => Output == MarkdownOutputKind.PlainText;


    /// <summary>
    /// Returns a value indicating whether <paramref name="element"/> is left out of the output.
    /// </summary>
    public bool IsIgnored(MarkdownElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        var ignored = IgnoredElementTypes;
        if (ignored.Count == 0)
            return false;

        for (var type = element.GetType(); type != null && typeof(MarkdownElement).IsAssignableFrom(type); type = type.BaseType)
            if (ignored.Contains(type))
                return true;

        return false;
    }

    internal IEnumerable<T> GetVisible<T>(IEnumerable<T> elements) where T : MarkdownElement
    {
        return IgnoredElementTypes.Count == 0
            ? elements
            : elements.Where(element => !IsIgnored(element));
    }

    /// <summary>
    /// Rejects further changes to the instance. Writing a document does this on its own.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// An ignored type does not derive from <see cref="MarkdownElement"/>.
    /// </exception>
    public void MakeReadOnly()
    {
        if (IsReadOnly)
            return;

        foreach (var type in IgnoredElementTypes)
            if (type == null || !typeof(MarkdownElement).IsAssignableFrom(type))
                throw new InvalidOperationException($"Ignored type '{type?.FullName ?? "null"}' does not derive from {nameof(MarkdownElement)}.");

        IgnoredElementTypes = new ReadOnlyTypeSet(IgnoredElementTypes);
        IsReadOnly = true;
    }
    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
            throw new InvalidOperationException("The format is read-only because it has already been used for writing.");
    }
    private static MarkdownFormat CreateReadOnly(MarkdownFormat format)
    {
        format.MakeReadOnly();
        return format;
    }
}

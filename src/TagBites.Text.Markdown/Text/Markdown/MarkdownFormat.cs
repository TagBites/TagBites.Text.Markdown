using TagBites.Collections;

namespace TagBites.Text.Markdown;

public sealed class MarkdownFormat
{
    public static MarkdownFormat Default { get; } = CreateReadOnly(new MarkdownFormat());
    public static MarkdownFormat PlainText { get; } = CreateReadOnly(new MarkdownFormat { Output = MarkdownOutputKind.PlainText });

    public bool IsReadOnly { get; private set; }

    public MarkdownOutputKind Output
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    }
    public ISet<Type> IgnoredElementTypes
    {
        get => field ??= new HashSet<Type>();
        set
        {
            ThrowIfReadOnly();
            field = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
    public MarkdownHeaderAnchorStyle HeaderAnchorStyle
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    }
    public bool SeparateLooseListItems
    {
        get;
        set
        {
            ThrowIfReadOnly();
            field = value;
        }
    } = true;

    public bool IsPlainText => Output == MarkdownOutputKind.PlainText;


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

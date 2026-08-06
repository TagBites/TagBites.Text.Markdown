namespace TagBites.Text.Markdown;

/// <summary>
/// A list of items, written with the <c>-</c> marker.
/// </summary>
public class MarkdownList : MarkdownElement
{
    /// <summary>
    /// Gets or sets a value indicating whether the items are numbered.
    /// </summary>
    /// <remarks>Default: <c>false</c>, which writes the <c>-</c> marker.</remarks>
    public bool IsOrdered { get; set; }
    /// <summary>
    /// Gets or sets the number the first item carries.
    /// </summary>
    /// <remarks>
    /// Applies only when <see cref="IsOrdered"/> is <c>true</c>, and continues a list that another block
    /// interrupted. Default: <c>1</c>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int StartNumber
    {
        get;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            field = value;
        }
    } = 1;

    /// <summary>
    /// Gets the items of the list.
    /// </summary>
    public IList<MarkdownListItem> Items { get; } = new List<MarkdownListItem>();


    /// <summary>
    /// Adds an item and returns it.
    /// </summary>
    public MarkdownListItem AddItem(MarkdownText text)
    {
        var item = new MarkdownListItem(text);
        Items.Add(item);
        return item;
    }
    /// <summary>
    /// Adds an item with a check box and returns it.
    /// </summary>
    /// <remarks>One list holds items with and without a check box, which is what the spec allows.</remarks>
    public MarkdownListItem AddCheckItem(bool isChecked, MarkdownText text)
    {
        var item = new MarkdownListItem(text) { IsChecked = isChecked };
        Items.Add(item);
        return item;
    }

    /// <inheritdoc />
    protected internal override void Resolve(MarkdownRenderer renderer)
    {
        var format = renderer.Format;
        var separate = format.SeparateLooseListItems && !format.IsPlainText && IsLoose(format);
        var index = 0;

        foreach (var item in format.GetVisible(Items))
        {
            if (index > 0)
            {
                renderer.AppendLine();

                if (separate)
                    renderer.AppendLine();
            }

            var marker = IsOrdered
                ? (StartNumber + index).ToString() + ". "
                : format.IsPlainText ? string.Empty : "- ";

            index++;

            renderer.Append(marker);

            // The content of the item lines up with its text, which is the column after the marker
            renderer.PushPrefix(new string(' ', marker.Length));
            item.Resolve(renderer);
            renderer.PopPrefix();
        }
    }

    private bool IsLoose(MarkdownFormat format)
    {
        foreach (var item in format.GetVisible(Items))
            if (item.HoldsBlocks(format))
                return true;

        return false;
    }
}

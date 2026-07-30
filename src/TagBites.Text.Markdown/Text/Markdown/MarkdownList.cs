namespace TagBites.Text.Markdown;

/// <summary>
/// A list of items, written with the <c>-</c> marker.
/// </summary>
public class MarkdownList : MarkdownElement
{
    /// <summary>
    /// Gets or sets a value indicating whether the items are numbered from one.
    /// </summary>
    /// <remarks>Default: <c>false</c>, which writes the <c>-</c> marker.</remarks>
    public bool IsOrdered { get; set; }

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
    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        var format = builder.Format;
        var separate = format.SeparateLooseListItems && !format.IsPlainText && IsLoose(format);
        var number = 0;

        foreach (var item in format.GetVisible(Items))
        {
            if (number > 0)
            {
                builder.AppendLine();

                if (separate)
                    builder.AppendLine();
            }

            number++;

            var marker = IsOrdered
                ? number.ToString() + ". "
                : format.IsPlainText ? string.Empty : "- ";

            builder.Append(marker);

            // The content of the item lines up with its text, which is the column after the marker
            builder.PushIndent(marker.Length);
            item.Resolve(builder);
            builder.PopIndent();
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

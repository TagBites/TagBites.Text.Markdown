namespace TagBites.Text.Markdown;

public class MarkdownList : MarkdownElement
{
    public bool IsOrdered { get; set; }

    public IList<MarkdownListItem> Items { get; } = new List<MarkdownListItem>();


    public MarkdownListItem AddItem(MarkdownText text)
    {
        var item = new MarkdownListItem(text);
        Items.Add(item);
        return item;
    }
    public MarkdownListItem AddCheckItem(bool isChecked, MarkdownText text)
    {
        var item = new MarkdownListItem(text) { IsChecked = isChecked };
        Items.Add(item);
        return item;
    }

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

namespace TagBites.Text.Markdown;

public class MarkdownListItem(MarkdownText text) : MarkdownContentElement
{
    public MarkdownText Text { get; } = text;

    public bool? IsChecked { get; set; }


    public MarkdownListItem AddChildItem(MarkdownText text) => AddCore(new MarkdownListItem(text));
    public MarkdownListItem AddCheckChildItem(bool isChecked, MarkdownText text) => AddCore(new MarkdownListItem(text) { IsChecked = isChecked });
    public T AddChildItem<T>(T item) where T : MarkdownListItem => AddCore(item);

    // A nested item and a nested list stay on the next line, every other block needs a blank one,
    // which is what makes the surrounding list loose.
    internal bool HoldsBlocks(MarkdownFormat format)
    {
        var content = ContentCore;
        if (content == null)
            return false;

        foreach (var element in format.GetVisible(content))
            if (element is not MarkdownListItem and not MarkdownList)
                return true;

        return false;
    }

    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        WriteCheckBox(builder);
        builder.Append(Text);

        var content = ContentCore;
        if (content == null || content.Count == 0)
            return;

        foreach (var element in builder.Format.GetVisible(content))
        {
            builder.AppendLine();

            // Nested items and lists stay tight, other blocks need a blank line
            if (element is MarkdownListItem child)
            {
                var marker = builder.Format.IsPlainText ? string.Empty : "- ";

                builder.Append(marker);
                builder.PushIndent(marker.Length);
                child.Resolve(builder);
                builder.PopIndent();
                continue;
            }

            if (element is not MarkdownList)
                builder.AppendLine();

            element.Resolve(builder);
        }
    }

    private void WriteCheckBox(MarkdownStringBuilder builder)
    {
        if (IsChecked == null)
            return;

        if (builder.Format.IsPlainText)
            builder.Append(IsChecked.Value ? "☑ " : "☐ ");
        else
            builder.Append(IsChecked.Value ? "[x] " : "[ ] ");
    }
}

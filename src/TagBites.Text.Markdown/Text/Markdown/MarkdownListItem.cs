namespace TagBites.Text.Markdown;

/// <summary>
/// A single item of a list, which also holds block elements of its own.
/// </summary>
public class MarkdownListItem(MarkdownText text) : MarkdownContentElement
{
    /// <summary>
    /// Gets or sets the text of the item, written on the line that carries the marker.
    /// </summary>
    public MarkdownText Text { get; set; } = text;

    /// <summary>
    /// Gets or sets the state of the check box, or <c>null</c> when the item carries none.
    /// </summary>
    /// <remarks>
    /// The check box comes from the GitHub Flavored Markdown task list extension, which puts it on
    /// the item rather than on the list, so one list holds items with and without it.
    /// Plain text output writes <c>☑</c> or <c>☐</c>, because the state is part of the meaning.
    /// </remarks>
    public bool? IsChecked { get; set; }


    /// <summary>
    /// Adds a nested item and returns it.
    /// </summary>
    public MarkdownListItem AddChildItem(MarkdownText text) => AddCore(new MarkdownListItem(text));
    /// <summary>
    /// Adds a nested item with a check box and returns it.
    /// </summary>
    public MarkdownListItem AddCheckChildItem(bool isChecked, MarkdownText text) => AddCore(new MarkdownListItem(text) { IsChecked = isChecked });
    /// <summary>
    /// Adds a nested item that is already built and returns it.
    /// </summary>
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

    /// <inheritdoc />
    protected internal override void Resolve(MarkdownRenderer renderer)
    {
        WriteCheckBox(renderer);
        renderer.Append(Text);

        var content = ContentCore;
        if (content == null || content.Count == 0)
            return;

        foreach (var element in renderer.Format.GetVisible(content))
        {
            var start = renderer.Length;
            renderer.AppendLine();

            // Nested items and lists stay tight, other blocks need a blank line
            if (element is MarkdownListItem child)
            {
                var marker = renderer.Format.IsPlainText ? string.Empty : "- ";

                renderer.Append(marker);
                renderer.PushPrefix(new string(' ', marker.Length));
                child.Resolve(renderer);
                renderer.PopPrefix();
                continue;
            }

            if (element is not MarkdownList)
                renderer.AppendLine();

            var separated = renderer.Length;
            element.Resolve(renderer);

            if (renderer.Length == separated)
                renderer.Truncate(start);
        }
    }

    private void WriteCheckBox(MarkdownRenderer renderer)
    {
        if (IsChecked == null)
            return;

        if (renderer.Format.IsPlainText)
            renderer.Append(IsChecked.Value ? "☑ " : "☐ ");
        else
            renderer.Append(IsChecked.Value ? "[x] " : "[ ] ");
    }
}

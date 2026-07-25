namespace TagBites.Text.Markdown;

public static class MarkdownElementExtensions
{
    public static T WithSection<T>(this T content, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddSection(text);
        return content;
    }
    public static T WithSection<T>(this T content, int level, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddSection(level, text);
        return content;
    }
    public static T WithHeader<T>(this T content, int level, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddHeader(level, text);
        return content;
    }
    public static T WithParagraph<T>(this T content, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddParagraph(text);
        return content;
    }
    public static T WithCode<T>(this T content, string code) where T : MarkdownContentElement
    {
        content.AddCode(code);
        return content;
    }
    public static T WithCode<T>(this T content, string language, string code) where T : MarkdownContentElement
    {
        content.AddCode(language, code);
        return content;
    }
    public static T WithQuote<T>(this T content, MarkdownText quote) where T : MarkdownContentElement
    {
        content.AddQuote(quote);
        return content;
    }
    public static T WithThematicBreak<T>(this T content) where T : MarkdownContentElement
    {
        content.AddThematicBreak();
        return content;
    }
    public static T WithElement<T>(this T content, MarkdownElement element) where T : MarkdownContentElement
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        content.AddCore(element);
        return content;
    }

    public static T WithItem<T>(this T list, MarkdownText text) where T : MarkdownList
    {
        list.AddItem(text);
        return list;
    }
    public static T WithCheckItem<T>(this T list, bool isChecked, MarkdownText text) where T : MarkdownList
    {
        list.AddCheckItem(isChecked, text);
        return list;
    }
    public static T WithItem<T>(this T list, MarkdownListItem item) where T : MarkdownList
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        list.Items.Add(item);
        return list;
    }

    public static T WithChildItem<T>(this T item, MarkdownText text) where T : MarkdownListItem
    {
        item.AddChildItem(text);
        return item;
    }
    public static T WithCheckChildItem<T>(this T item, bool isChecked, MarkdownText text) where T : MarkdownListItem
    {
        item.AddCheckChildItem(isChecked, text);
        return item;
    }
    public static T WithChildItem<T>(this T item, MarkdownListItem child) where T : MarkdownListItem
    {
        item.AddChildItem(child);
        return item;
    }
}

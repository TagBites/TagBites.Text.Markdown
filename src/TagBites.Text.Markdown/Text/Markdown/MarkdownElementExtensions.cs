namespace TagBites.Text.Markdown;

/// <summary>
/// Appends an element to the element the method is called on.
/// </summary>
/// <remarks>
/// Every method returns the element it was called on, with the type it was called with, so a chain keeps its type.
/// A method exists only where a single call produces a complete element.
/// <see cref="WithElement{T}"/> appends a list, a table or an empty quote, which need configuring first.
/// </remarks>
public static class MarkdownElementExtensions
{
    /// <summary>
    /// Adds a section one level below <paramref name="content"/>.
    /// </summary>
    public static T WithSection<T>(this T content, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddSection(text);
        return content;
    }
    /// <summary>
    /// Adds a section whose header carries an explicit level.
    /// </summary>
    public static T WithSection<T>(this T content, int level, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddSection(level, text);
        return content;
    }
    /// <summary>
    /// Adds a header with an explicit level.
    /// </summary>
    public static T WithHeader<T>(this T content, int level, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddHeader(level, text);
        return content;
    }
    /// <summary>
    /// Adds a paragraph.
    /// </summary>
    public static T WithParagraph<T>(this T content, MarkdownText text) where T : MarkdownContentElement
    {
        content.AddParagraph(text);
        return content;
    }
    /// <summary>
    /// Adds a code block without a language.
    /// </summary>
    public static T WithCode<T>(this T content, string code) where T : MarkdownContentElement
    {
        content.AddCode(code);
        return content;
    }
    /// <summary>
    /// Adds a code block written in <paramref name="language"/>.
    /// </summary>
    public static T WithCode<T>(this T content, string language, string code) where T : MarkdownContentElement
    {
        content.AddCode(language, code);
        return content;
    }
    /// <inheritdoc cref="MarkdownContentElement.AddHtml"/>
    public static T WithHtml<T>(this T content, string html) where T : MarkdownContentElement
    {
        content.AddHtml(html);
        return content;
    }
    /// <summary>
    /// Adds a quote that starts with <paramref name="quote"/>.
    /// </summary>
    public static T WithQuote<T>(this T content, MarkdownText quote) where T : MarkdownContentElement
    {
        content.AddQuote(quote);
        return content;
    }
    /// <summary>
    /// Adds a thematic break.
    /// </summary>
    public static T WithThematicBreak<T>(this T content) where T : MarkdownContentElement
    {
        content.AddThematicBreak();
        return content;
    }
    /// <summary>
    /// Adds an element that is already built.
    /// </summary>
    public static T WithElement<T>(this T content, MarkdownElement element) where T : MarkdownContentElement
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        content.AddCore(element);
        return content;
    }

    /// <summary>
    /// Adds an item to <paramref name="list"/>.
    /// </summary>
    public static T WithItem<T>(this T list, MarkdownText text) where T : MarkdownList
    {
        list.AddItem(text);
        return list;
    }
    /// <summary>
    /// Adds an item with a check box to <paramref name="list"/>.
    /// </summary>
    public static T WithCheckItem<T>(this T list, bool isChecked, MarkdownText text) where T : MarkdownList
    {
        list.AddCheckItem(isChecked, text);
        return list;
    }
    /// <summary>
    /// Adds an item that is already built.
    /// </summary>
    public static T WithItem<T>(this T list, MarkdownListItem item) where T : MarkdownList
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        list.Items.Add(item);
        return list;
    }

    /// <summary>
    /// Adds an item nested under <paramref name="item"/>, which produces siblings.
    /// </summary>
    public static T WithChildItem<T>(this T item, MarkdownText text) where T : MarkdownListItem
    {
        item.AddChildItem(text);
        return item;
    }
    /// <summary>
    /// Adds an item with a check box nested under <paramref name="item"/>, which produces siblings.
    /// </summary>
    public static T WithCheckChildItem<T>(this T item, bool isChecked, MarkdownText text) where T : MarkdownListItem
    {
        item.AddCheckChildItem(isChecked, text);
        return item;
    }
    /// <summary>
    /// Adds an item that is already built, nested under <paramref name="item"/>.
    /// </summary>
    public static T WithChildItem<T>(this T item, MarkdownListItem child) where T : MarkdownListItem
    {
        item.AddChildItem(child);
        return item;
    }
}

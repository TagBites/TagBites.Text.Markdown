namespace TagBites.Text.Markdown
{
    public static class MarkdownUnitExtensions
    {
        public static T WithParagraph<T>(this T content, string text) where T : MarkdownContentElement
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
        public static T WithQuote<T>(this T content, string quote) where T : MarkdownContentElement
        {
            content.AddQuote(quote);
            return content;
        }

        public static T With<T>(this T content, MarkdownParagraph paragraph) where T : MarkdownContentElement
        {
            content.AddCore(paragraph);
            return content;
        }
        public static T With<T>(this T content, MarkdownCode code) where T : MarkdownContentElement
        {
            content.AddCore(code);
            return content;
        }
        public static T With<T>(this T content, MarkdownQuote quote) where T : MarkdownContentElement
        {
            content.AddCore(quote);
            return content;
        }
        public static T With<T>(this T content, MarkdownList list) where T : MarkdownContentElement
        {
            content.AddCore(list);
            return content;
        }
        public static T With<T>(this T content, MarkdownOrderedList list) where T : MarkdownContentElement
        {
            content.AddCore(list);
            return content;
        }
        public static T With<T>(this T content, MarkdownCheckList list) where T : MarkdownContentElement
        {
            content.AddCore(list);
            return content;
        }
        public static T With<T>(this T content, MarkdownTable table) where T : MarkdownContentElement
        {
            content.AddCore(table);
            return content;
        }

        public static T WithElement<T>(this T content, string text) where T : MarkdownList
        {
            content.AddElement(text);
            return content;
        }
        public static T WithElement<T>(this T content, bool isChecked, string text) where T : MarkdownCheckList
        {
            content.AddElement(isChecked, text);
            return content;
        }
        public static T WithElement<T>(this T content, MarkdownListElement element) where T : MarkdownList
        {
            content.Elements.Add(element);
            return content;
        }
        public static T WithChildElement<T>(this T content, string text) where T : MarkdownListElement
        {
            content.AddCore(new MarkdownListElement(text));
            return content;
        }
        public static T WithChildElement<T>(this T content, bool isChecked, string text) where T : MarkdownCheckListElement
        {
            content.AddCore(new MarkdownCheckListElement(isChecked, text));
            return content;
        }
        public static T WithChildElement<T>(this T content, MarkdownListElement element) where T : MarkdownListElement
        {
            content.AddCore(element);
            return content;
        }
    }
}

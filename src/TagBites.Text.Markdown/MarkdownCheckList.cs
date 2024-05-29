namespace TagBites.Text.Markdown
{
    public class MarkdownCheckList : MarkdownList
    {
        protected override bool IsCheckList => true;


        public MarkdownListElement AddElement(bool isChecked, string text)
        {
            var element = new MarkdownCheckListElement(isChecked, text);
            Elements.Add(element);
            return element;
        }
    }
}

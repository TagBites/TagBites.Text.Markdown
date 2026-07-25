namespace TagBites.Text.Markdown.Tests;

public class ThematicBreakTests : MarkdownTestBase
{
    [Fact]
    public void Standalone() => AssertMarkdown("---", new MarkdownThematicBreak());

    [Fact]
    public void BetweenParagraphs()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("before")
            .WithThematicBreak()
            .WithParagraph("after");

        var expected = Lines(
            "before",
            "",
            "---",
            "",
            "after");

        AssertMarkdown(expected, doc);
    }

    [Fact]
    public void UnderListItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddThematicBreak();

        var expected = Lines(
            "- a",
            "",
            "  ---");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void InQuote()
    {
        var quote = new MarkdownQuote("intro");
        quote.AddThematicBreak();

        var expected = Lines(
            "> intro",
            ">",
            "> ---");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void CleanTextIsEmpty() => AssertCleanText(string.Empty, new MarkdownThematicBreak());
}

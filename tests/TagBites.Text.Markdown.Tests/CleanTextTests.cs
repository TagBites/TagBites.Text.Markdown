namespace TagBites.Text.Markdown.Tests;

public class CleanTextTests : MarkdownTestBase
{
    [Fact]
    public void Header() => AssertCleanText("Test", new MarkdownHeader(1, "Test").SetCustomId("my-id"));

    [Fact]
    public void Quote() => AssertCleanText(Lines("line one", "line two"), new MarkdownQuote("line one\nline two"));

    [Fact]
    public void Code() => AssertCleanText("var x = 1;", new MarkdownCode("csharp", "var x = 1;"));

    [Fact]
    public void NestedQuote()
    {
        var quote = new MarkdownQuote("outer");
        quote.AddQuote("inner");

        var expected = Lines(
            "outer",
            "",
            "inner");

        AssertCleanText(expected, quote);
    }

    [Fact]
    public void ListInQuote()
    {
        var quote = new MarkdownQuote("intro");
        quote.AddList()
            .WithItem("a")
            .WithItem("b");

        var expected = Lines(
            "intro",
            "",
            "a",
            "b");

        AssertCleanText(expected, quote);
    }

    [Fact]
    public void TableCellNewLine()
    {
        var table = new MarkdownTable()
            .SetHeaders("a", "b")
            .WithRow("line one\nline two", "z");

        var expected = Lines(
            "a b",
            "line one line two z");

        AssertCleanText(expected, table);
    }

    [Fact]
    public void UnorderedList()
    {
        var list = new MarkdownList()
            .WithItem("a")
            .WithItem("b");

        var expected = Lines(
            "a",
            "b");

        AssertCleanText(expected, list);
    }

    [Fact]
    public void OrderedList()
    {
        var list = new MarkdownList { IsOrdered = true }
            .WithItem("a")
            .WithItem("b");

        var expected = Lines(
            "1. a",
            "2. b");

        AssertCleanText(expected, list);
    }

    [Fact]
    public void CheckList()
    {
        var list = new MarkdownList()
            .WithCheckItem(true, "done")
            .WithCheckItem(false, "todo");

        var expected = Lines(
            "☑ done",
            "☐ todo");

        AssertCleanText(expected, list);
    }

    [Fact]
    public void Table()
    {
        var table = new MarkdownTable()
            .SetHeaders("col1", "col2")
            .WithRow("a", "b")
            .WithRow("1", "2");

        var expected = Lines(
            "col1 col2",
            "a b",
            "1 2");

        AssertCleanText(expected, table);
    }

    [Fact]
    public void Document()
    {
        var doc = new MarkdownDocument()
            .WithHeader(1, "Title")
            .WithParagraph("text")
            .WithQuote("quote");

        var expected = Lines(
            "Title",
            "",
            "text",
            "",
            "quote");

        AssertCleanText(expected, doc);
    }
}

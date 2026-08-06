namespace TagBites.Text.Markdown.Tests;

public class QuoteTests : MarkdownTestBase
{
    [Fact]
    public void SingleLine() => AssertMarkdown("> quote", new MarkdownQuote("quote"));

    [Fact]
    public void MultiLine() => AssertMarkdown(Lines("> line one", "> line two"), new MarkdownQuote("line one\nline two"));

    [Fact]
    public void MultiLineWithCarriageReturn() => AssertMarkdown(Lines("> line one\r", "> line two"), new MarkdownQuote("line one\r\nline two"));

    [Fact]
    public void NullTextBecomesEmpty() => AssertMarkdown(string.Empty, new MarkdownQuote(null!));

    [Fact]
    public void ListInQuote()
    {
        var quote = new MarkdownQuote("intro");
        quote.AddList()
            .WithItem("a")
            .WithItem("b");

        var expected = Lines(
            "> intro",
            ">",
            "> - a",
            "> - b");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void CodeInQuote()
    {
        var quote = new MarkdownQuote("see");
        quote.AddCode("csharp", "var x = 1;");

        var expected = Lines(
            "> see",
            ">",
            "> ```csharp",
            "> var x = 1;",
            "> ```");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void TableInQuote()
    {
        var quote = new MarkdownQuote("data");
        quote.AddTable()
            .SetHeaders("a", "b")
            .WithRow("1", "2");

        var expected = Lines(
            "> data",
            ">",
            "> | a | b |",
            "> | - | - |",
            "> | 1 | 2 |");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void NestedQuote()
    {
        var quote = new MarkdownQuote("outer");
        quote.AddQuote("inner");

        var expected = Lines(
            "> outer",
            ">",
            "> > inner");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void DeeplyNestedQuote()
    {
        var quote = new MarkdownQuote("l1");
        var second = quote.AddQuote("l2");
        second.AddQuote("l3");

        var expected = Lines(
            "> l1",
            ">",
            "> > l2",
            "> >",
            "> > > l3");

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void QuoteWithoutText()
    {
        var quote = new MarkdownQuote();
        quote.AddParagraph("only block");

        var expected = "> only block";

        AssertMarkdown(expected, quote);
    }

    [Fact]
    public void QuoteInListItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("item");
        item.AddQuote("quoted");

        var expected = Lines(
            "- item",
            "",
            "  > quoted");

        AssertMarkdown(expected, list);
    }
}

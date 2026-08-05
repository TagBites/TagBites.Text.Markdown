namespace TagBites.Text.Markdown.Tests;

public class DocumentTests : MarkdownTestBase
{
    [Fact]
    public void EmptyDocument() => AssertMarkdown(string.Empty, new MarkdownDocument());

    [Fact]
    public void ReadmeExample()
    {
        var doc = new MarkdownDocument();
        doc.AddHeader(1, "My Document");
        doc.AddHeader(2, "Some section");

        doc.AddParagraph("Some table below.");

        var table = doc.AddTable();
        table.SetHeaders("col1", "col2", "col3");
        table.WithRow("a", "b", "c");
        table.WithRow("1", "2", "3");

        doc.AddParagraph("Some check list below.");

        var list = doc.AddList();
        list.AddCheckItem(true, "task 1");
        list.AddCheckItem(true, "task 2");
        list.AddCheckItem(false, "task 3");
        list.AddCheckItem(false, "task 4");

        var expected = Lines(
            "# My Document",
            "",
            "## Some section",
            "",
            "Some table below.",
            "",
            "| col1 | col2 | col3 |",
            "| ---- | ---- | ---- |",
            "| a    | b    | c    |",
            "| 1    | 2    | 3    |",
            "",
            "Some check list below.",
            "",
            "- [x] task 1",
            "- [x] task 2",
            "- [ ] task 3",
            "- [ ] task 4");

        AssertMarkdown(expected, doc);
    }

    [Fact]
    public void BlockSeparation()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("first")
            .WithParagraph("second")
            .WithQuote("quote")
            .WithCode("csharp", "var x = 1;")
            .WithParagraph("last");

        var expected = Lines(
            "first",
            "",
            "second",
            "",
            "> quote",
            "",
            "```csharp",
            "var x = 1;",
            "```",
            "",
            "last");

        AssertMarkdown(expected, doc);
    }

    [Fact]
    public void EmptyElementLeavesNoBlankLine()
    {
        var doc = new MarkdownDocument();
        doc.AddParagraph("before");
        doc.AddList();
        doc.AddTable();
        doc.AddParagraph("after");

        AssertMarkdown(Lines("before", "", "after"), doc);
    }

    [Fact]
    public void DocumentOfEmptyElementsRendersNothing()
    {
        var doc = new MarkdownDocument();
        doc.AddList();
        doc.AddTable();

        AssertMarkdown(string.Empty, doc);
    }

    [Fact]
    public void TableBetweenParagraphs()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("before")
            .WithElement(new MarkdownTable()
                .WithHeader("a")
                .WithRow("1"))
            .WithParagraph("after");

        var expected = Lines(
            "before",
            "",
            "| a |",
            "| - |",
            "| 1 |",
            "",
            "after");

        AssertMarkdown(expected, doc);
    }

    [Fact]
    public void ListAfterParagraph()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("intro")
            .WithElement(new MarkdownList()
                .WithItem("a")
                .WithItem("b"));

        var expected = Lines(
            "intro",
            "",
            "- a",
            "- b");

        AssertMarkdown(expected, doc);
    }
}

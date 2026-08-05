namespace TagBites.Text.Markdown.Tests;

public class FormatTests : MarkdownTestBase
{
    [Fact]
    public void IgnoredCodeBlockLeavesNoBlankLine()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("before")
            .WithCode("csharp", "var x = 1;")
            .WithParagraph("after");

        AssertIgnoring(Lines(
            "before",
            "",
            "after"), doc, typeof(MarkdownCode));
    }

    [Fact]
    public void IgnoredCodeBlockAtTheEndLeavesNoBlankLine()
    {
        var doc = new MarkdownDocument()
            .WithParagraph("only")
            .WithCode("csharp", "var x = 1;");

        AssertIgnoring("only", doc, typeof(MarkdownCode));
    }

    [Fact]
    public void CodeSpanSurvivesAnIgnoredCodeBlock()
    {
        var doc = new MarkdownDocument();
        doc.AddParagraph("call " + MarkdownText.Code("AddParagraph") + " first");
        doc.AddCode("csharp", "var x = 1;");

        AssertIgnoring("call `AddParagraph` first", doc, typeof(MarkdownCode));
    }

    [Fact]
    public void IgnoredElementIsDroppedInsideAQuote()
    {
        var doc = new MarkdownDocument();
        var quote = doc.AddQuote("intro");
        quote.AddCode("csharp", "var x = 1;");

        AssertIgnoring("> intro", doc, typeof(MarkdownCode));
    }

    [Fact]
    public void IgnoredElementIsDroppedUnderAListItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddCode("csharp", "var x = 1;");
        list.AddItem("b");

        AssertIgnoring(Lines(
            "- a",
            "- b"), list, typeof(MarkdownCode));
    }

    [Fact]
    public void PlainTextAndIgnoringCombine()
    {
        var doc = new MarkdownDocument()
            .WithHeader(1, "Title")
            .WithParagraph("text")
            .WithCode("csharp", "var x = 1;");

        var format = new MarkdownFormat
        {
            Output = MarkdownOutputKind.PlainText,
            IgnoredElementTypes = { typeof(MarkdownCode) }
        };

        Assert.Equal(Lines("Title", "", "text"), doc.ToString(format));
    }

    [Fact]
    public void DefaultsRenderMarkdown()
    {
        Assert.False(MarkdownFormat.Default.IsPlainText);
        Assert.True(MarkdownFormat.PlainText.IsPlainText);
        Assert.Empty(MarkdownFormat.Default.IgnoredElementTypes);
    }

    [Fact]
    public void WritingMakesTheFormatReadOnly()
    {
        var format = new MarkdownFormat();

        Assert.False(format.IsReadOnly);
        new MarkdownParagraph("a").ToString(format);

        Assert.True(format.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() => format.Output = MarkdownOutputKind.PlainText);
        Assert.Throws<NotSupportedException>(() => format.IgnoredElementTypes.Add(typeof(MarkdownCode)));
    }

    [Fact]
    public void PresetsAreReadOnly()
    {
        Assert.True(MarkdownFormat.Default.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => MarkdownFormat.PlainText.IgnoredElementTypes.Add(typeof(MarkdownCode)));
    }

    [Fact]
    public void NullFormatThrows() => Assert.Throws<ArgumentNullException>(() => new MarkdownParagraph("a").ToString(null!));
}

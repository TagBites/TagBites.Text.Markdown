namespace TagBites.Text.Markdown.Tests;

public class MarkdownTextTests : MarkdownTestBase
{
    [Theory]
    [InlineData("Roslyn-based C# expression parser")]
    [InlineData("snake_case_name stays intact")]
    [InlineData("a < b and Func<> stay intact")]
    [InlineData("version 2.0 released")]
    [InlineData("cost is 5 & 6")]
    [InlineData("done!")]
    [InlineData("a ~ b")]
    [InlineData("plain prose without syntax")]
    public void ProseIsLeftAlone(string text) => Assert.Equal(text, ((MarkdownText)text).Markdown);

    [Theory]
    [InlineData("[link]", "\\[link\\]")]
    [InlineData("2 * 3", "2 \\* 3")]
    [InlineData("a `code` b", "a \\`code\\` b")]
    [InlineData("back\\slash", "back\\\\slash")]
    [InlineData("<br>", "\\<br>")]
    [InlineData("</div>", "\\</div>")]
    [InlineData("&amp; entity", "\\&amp; entity")]
    [InlineData("![image]", "\\!\\[image\\]")]
    [InlineData("a ~~b~~ c", "a \\~~b\\~~ c")]
    [InlineData("_leading underscore", "\\_leading underscore")]
    public void InlineSyntaxIsEscaped(string text, string expected) => Assert.Equal(expected, ((MarkdownText)text).Markdown);

    [Theory]
    [InlineData("# heading", "\\# heading")]
    [InlineData("> quote", "\\> quote")]
    [InlineData("- item", "\\- item")]
    [InlineData("+ item", "\\+ item")]
    [InlineData("1. item", "1\\. item")]
    [InlineData("12) item", "12\\) item")]
    [InlineData("---", "\\---")]
    [InlineData("***", "\\*\\*\\*")]
    [InlineData("  - indented item", "  \\- indented item")]
    public void BlockMarkersAreEscapedAtLineStart(string text, string expected) => Assert.Equal(expected, ((MarkdownText)text).Markdown);

    [Theory]
    [InlineData("issue #42 is open")]
    [InlineData("a - b - c")]
    [InlineData("see item 1. of the list")]
    [InlineData("x > y")]
    public void BlockMarkersAreLeftAloneInsideALine(string text) => Assert.Equal(text, ((MarkdownText)text).Markdown);

    [Theory]
    // A setext underline turns the line above into a heading
    [InlineData("Title\n===", "Title\n\\===")]
    [InlineData("Title\n=", "Title\n\\=")]
    [InlineData("Title\n--", "Title\n\\--")]
    [InlineData("Title\n---", "Title\n\\---")]
    // A space carries no backslash escape, so an entity breaks the run
    [InlineData("    indented", "&#32;   indented")]
    [InlineData("\tindented", "&#9;indented")]
    [InlineData("   three spaces", "   three spaces")]
    // A blank line would end the block
    [InlineData("a\n\nb", "a\n&#32;\nb")]
    [InlineData("a\n  \nb", "a\n&#32;  \nb")]
    public void TextCannotEndOrRetypeTheBlock(string text, string expected) => Assert.Equal(expected, ((MarkdownText)text).Markdown);

    [Theory]
    // A lone line has no line above it in the block, so it underlines nothing
    [InlineData("===", "===")]
    // The same line is still a thematic break
    [InlineData("---", "\\---")]
    public void ALoneLineIsNotASetextUnderline(string text, string expected) => Assert.Equal(expected, ((MarkdownText)text).Markdown);

    [Fact]
    public void EachLineIsCheckedSeparately()
    {
        var text = (MarkdownText)"first line\n# second line";

        Assert.Equal("first line\n\\# second line", text.Markdown);
        Assert.Equal("first line\n# second line", text.Text);
    }

    [Fact]
    public void RawIsNotEscaped()
    {
        var text = MarkdownText.Raw("**bold**");

        Assert.Equal("**bold**", text.Markdown);
        Assert.Equal("**bold**", text.Text);
    }

    [Fact]
    public void RawKeepsSeparateCleanText()
    {
        var text = MarkdownText.Raw("**bold**", "bold");

        Assert.Equal("**bold**", text.Markdown);
        Assert.Equal("bold", text.Text);
    }

    [Fact]
    public void ConcatenationKeepsBothForms()
    {
        var text = "see [docs]" + MarkdownText.Raw(" [here](x.md)");

        Assert.Equal("see \\[docs\\] [here](x.md)", text.Markdown);
        Assert.Equal("see [docs] [here](x.md)", text.Text);
    }

    [Fact]
    public void LineBreakJoinsTwoLines()
    {
        var text = "line one" + MarkdownText.LineBreak + "line two";

        Assert.Equal("line one  \nline two", text.Markdown);
        Assert.Equal("line one\nline two", text.Text);
    }

    [Fact]
    public void NullBecomesEmpty()
    {
        var text = (MarkdownText)null!;

        Assert.True(text.IsEmpty);
        Assert.Equal(string.Empty, text.Markdown);
    }

    [Fact]
    public void UntrustedTextCannotIntroduceMarkup()
    {
        var doc = new MarkdownDocument();
        doc.AddParagraph("Report by [admin](https://link.example) **now**");

        AssertMarkdown("Report by \\[admin\\](https://link.example) \\*\\*now\\*\\*", doc);
    }

    [Fact]
    public void CleanTextReturnsTheOriginal()
    {
        var doc = new MarkdownDocument();
        doc.AddParagraph("Report by [admin](https://link.example) **now**");

        AssertCleanText("Report by [admin](https://link.example) **now**", doc);
    }
}

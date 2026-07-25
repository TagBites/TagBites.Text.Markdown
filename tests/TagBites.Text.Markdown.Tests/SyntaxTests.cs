namespace TagBites.Text.Markdown.Tests;

public class SyntaxTests : MarkdownTestBase
{
    [Theory]
    [InlineData("text", "**text**")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Bold(string? text, string expected) => Assert.Equal(expected, MarkdownText.Bold(text!).Markdown);

    [Theory]
    [InlineData("text", "_text_")]
    [InlineData("", "")]
    public void Italic(string text, string expected) => Assert.Equal(expected, MarkdownText.Italic(text).Markdown);

    [Theory]
    [InlineData("text", "~~text~~")]
    [InlineData("", "")]
    public void Strikethrough(string text, string expected) => Assert.Equal(expected, MarkdownText.Strikethrough(text).Markdown);

    [Theory]
    [InlineData("var x;", "`var x;`")]
    [InlineData("a * b [c]", "`a * b [c]`")]
    [InlineData("", "")]
    [InlineData("a ` b", "``a ` b``")]
    [InlineData("`leading", "`` `leading ``")]
    [InlineData("trailing`", "`` trailing` ``")]
    [InlineData("``double``", "``` ``double`` ```")]
    [InlineData("```", "```` ``` ````")]
    [InlineData(" spaced ", "`  spaced  `")]
    public void Code(string code, string expected) => Assert.Equal(expected, MarkdownText.Code(code).Markdown);

    [Theory]
    [InlineData("name", "https://tagbites.com", "[name](https://tagbites.com)")]
    [InlineData("", "https://tagbites.com", "[](https://tagbites.com)")]
    [InlineData("a [b]", "x.md", "[a \\[b\\]](x.md)")]
    [InlineData("a", "x_y.md", "[a](x_y.md)")]
    [InlineData("a", "x(y).md", "[a](x(y).md)")]
    public void Link(string name, string address, string expected) => Assert.Equal(expected, MarkdownText.Link(name, address).Markdown);

    [Theory]
    [InlineData("logo", "logo.png", "![logo](logo.png)")]
    [InlineData("a *b*", "x.png", "![a \\*b\\*](x.png)")]
    public void Image(string name, string address, string expected) => Assert.Equal(expected, MarkdownText.Image(name, address).Markdown);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LinkWithoutAddressThrows(string? address) => Assert.Throws<ArgumentException>(() => MarkdownText.Link("name", address!));

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ImageWithoutAddressThrows(string? address) => Assert.Throws<ArgumentException>(() => MarkdownText.Image("name", address!));

    [Fact]
    public void ResultIsNotEscapedAgain() => AssertMarkdown("**total**", new MarkdownParagraph(MarkdownText.Bold("total")));

    [Fact]
    public void CleanTextDropsSyntax()
    {
        var text = MarkdownText.Bold("total") + " see " + MarkdownText.Link("guide", "x.md");

        Assert.Equal("**total** see [guide](x.md)", text.Markdown);
        Assert.Equal("total see guide", text.Text);
    }
}

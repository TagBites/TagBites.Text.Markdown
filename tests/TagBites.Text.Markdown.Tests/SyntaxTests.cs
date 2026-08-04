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
    [InlineData("logo", "a logo.png", "![logo](a%20logo.png)")]
    public void Image(string name, string address, string expected) => Assert.Equal(expected, MarkdownText.Image(name, address).Markdown);

    [Theory]
    [InlineData("a b.md", "a%20b.md")]
    [InlineData("a\tb.md", "a%09b.md")]
    [InlineData("a\nb.md", "a%0Ab.md")]
    [InlineData("a\rb.md", "a%0Db.md")]
    // The spec forbids every ASCII control character, not only the three that end a line
    [InlineData("a\u0001b.md", "a%01b.md")]
    [InlineData("a\u001Fb.md", "a%1Fb.md")]
    [InlineData("a\u007Fb.md", "a%7Fb.md")]
    [InlineData("a\u0080b.md", "a%C2%80b.md")]
    [InlineData("a\u009Fb.md", "a%C2%9Fb.md")]
    // A parenthesis is allowed backslash-escaped, which keeps it readable in the address
    [InlineData("x(.md", "x\\(.md")]
    [InlineData("x).md", "x\\).md")]
    [InlineData("x(a(b).md", "x\\(a\\(b\\).md")]
    [InlineData("Foo_(bar).md", "Foo_(bar).md")]
    [InlineData("x((1)).md", "x((1)).md")]
    [InlineData("a&b<c>d\"e.md", "a&b<c>d\"e.md")]
    [InlineData("already%20encoded.md", "already%20encoded.md")]
    // A letter outside the C1 range needs no encoding, so the address stays readable
    [InlineData("zażółć-gęślą.md", "zażółć-gęślą.md")]
    [InlineData("docs/中文.md", "docs/中文.md")]
    [InlineData("café.md", "café.md")]
    public void AddressKeepsTheLinkParsable(string address, string expected)
    {
        Assert.Equal($"[name]({expected})", MarkdownText.Link("name", address).Markdown);
    }

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

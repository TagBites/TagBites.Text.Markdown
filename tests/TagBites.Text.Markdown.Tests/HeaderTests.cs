namespace TagBites.Text.Markdown.Tests;

public class HeaderTests : MarkdownTestBase
{
    [Theory]
    [InlineData(1, "# Test")]
    [InlineData(2, "## Test")]
    [InlineData(3, "### Test")]
    [InlineData(6, "###### Test")]
    public void HeaderLevels(int level, string expected) => AssertMarkdown(expected, new MarkdownHeader(level, "Test"));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    public void InvalidLevels(int level) => Assert.Throws<ArgumentOutOfRangeException>(() => new MarkdownHeader(level, "Test"));

    [Theory]
    [InlineData("my-id", "# <a id=\"my-id\"></a> Test")]
    [InlineData("#my-id", "# <a id=\"my-id\"></a> Test")]
    public void CustomId(string customId, string expected) => AssertMarkdown(expected, new MarkdownHeader(1, "Test").SetCustomId(customId));

    [Theory]
    [InlineData("my-id", "# Test{#my-id}")]
    [InlineData("#my-id", "# Test{#my-id}")]
    public void AttributeAnchorStyle(string customId, string expected)
    {
        AssertMarkdown(expected, new MarkdownHeader(1, "Test").SetCustomId(customId), new MarkdownFormat { HeaderAnchorStyle = MarkdownHeaderAnchorStyle.Attribute });
    }

    [Theory]
    [InlineData(MarkdownHeaderAnchorStyle.Attribute)]
    [InlineData(MarkdownHeaderAnchorStyle.HtmlAnchor)]
    public void HeaderWithoutAnchorIgnoresTheStyle(MarkdownHeaderAnchorStyle style)
    {
        AssertMarkdown("# Test", new MarkdownHeader(1, "Test"), new MarkdownFormat { HeaderAnchorStyle = style });
    }

    [Theory]
    [InlineData("a}b")]
    [InlineData("a{b")]
    [InlineData("a b")]
    [InlineData("a\nb")]
    [InlineData("a\"b")]
    [InlineData("a<b")]
    public void InvalidCustomIdThrows(string customId) => Assert.Throws<ArgumentException>(() => new MarkdownHeader(1, "Test").SetCustomId(customId));

    [Fact]
    public void HeaderInQuote()
    {
        var quote = new MarkdownQuote();
        quote.AddHeader(2, "Section");

        AssertMarkdown("> ## Section", quote);
    }

    [Fact]
    public void HeaderUnderListItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddHeader(3, "Sub");

        var expected = Lines(
            "- a",
            "",
            "  ### Sub");

        AssertMarkdown(expected, list);
    }
}

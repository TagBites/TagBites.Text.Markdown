namespace TagBites.Text.Markdown.Tests;

public class AnchorTests : MarkdownTestBase
{
    [Theory]
    [InlineData("Usage", "usage")]
    [InlineData("Some section", "some-section")]
    [InlineData("Roslyn-based C# parser", "roslyn-based-c-parser")]
    [InlineData("snake_case_name", "snake_case_name")]
    [InlineData("...", null)]
    [InlineData("", null)]
    public void AnchorIdFollowsTheText(string text, string? expected) => Assert.Equal(expected, new MarkdownHeader(1, text).AnchorId);

    [Fact]
    public void CustomIdReplacesTheDefault() => Assert.Equal("custom-id", new MarkdownHeader(1, "Usage").SetCustomId("#custom-id").AnchorId);

    [Fact]
    public void LinkToASectionUsesItsText()
    {
        var doc = new MarkdownDocument();
        var section = doc.AddSection("Some section");
        section.AddParagraph("See " + MarkdownText.Link(section) + " above.");

        AssertMarkdown(Lines(
            "# Some section",
            "",
            "See [Some section](#some-section) above."), doc);
    }

    [Fact]
    public void LinkTakesItsOwnText()
    {
        var header = new MarkdownHeader(2, "Options");

        Assert.Equal("[see below](#options)", MarkdownText.Link("see below", header).Markdown);
    }

    [Fact]
    public void LinkWithoutAnAnchorThrows() => Assert.Throws<ArgumentException>(() => MarkdownText.Link(new MarkdownHeader(1, "...")));
}

namespace TagBites.Text.Markdown.Tests;

public class ParagraphTests : MarkdownTestBase
{
    [Fact]
    public void SingleLine() => AssertMarkdown("text", new MarkdownParagraph("text"));

    [Fact]
    public void MultiLine() => AssertMarkdown(Lines("line one", "line two"), new MarkdownParagraph("line one\nline two"));

    [Fact]
    public void NullTextBecomesEmpty() => AssertMarkdown(string.Empty, new MarkdownParagraph(null!));
}

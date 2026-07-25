namespace TagBites.Text.Markdown.Tests;

public abstract class MarkdownTestBase
{
    protected static void AssertMarkdown(string expected, MarkdownElement element) => Assert.Equal(expected, element.ToString());
    protected static void AssertMarkdown(string expected, MarkdownElement element, MarkdownFormat format) => Assert.Equal(expected, element.ToString(format));
    protected static void AssertCleanText(string expected, MarkdownElement element) => Assert.Equal(expected, element.ToString(MarkdownFormat.PlainText));
    protected static void AssertIgnoring(string expected, MarkdownElement element, params Type[] ignored)
    {
        Assert.Equal(expected, element.ToString(new MarkdownFormat { IgnoredElementTypes = new HashSet<Type>(ignored) }));
    }

    protected static string Lines(params string[] lines) => string.Join("\n", lines);
}

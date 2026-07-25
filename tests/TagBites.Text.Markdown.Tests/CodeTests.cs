namespace TagBites.Text.Markdown.Tests;

public class CodeTests : MarkdownTestBase
{
    [Theory]
    [InlineData(null, "var x = 1;", "```\nvar x = 1;\n```")]
    [InlineData("", "var x = 1;", "```\nvar x = 1;\n```")]
    [InlineData("csharp", "var x = 1;", "```csharp\nvar x = 1;\n```")]
    [InlineData(null, "var x = 1;\nvar y = 2;", "```\nvar x = 1;\nvar y = 2;\n```")]
    [InlineData(null, "", "```\n\n```")]
    [InlineData(null, "a ` b", "```\na ` b\n```")]
    [InlineData(null, "```\nnested\n```", "````\n```\nnested\n```\n````")]
    [InlineData("csharp", "````\nlonger\n````", "`````csharp\n````\nlonger\n````\n`````")]
    public void CodeBlock(string? language, string code, string expected) => AssertMarkdown(expected, new MarkdownCode(language, code));

    [Theory]
    [InlineData(null, "var x = 1;", "var x = 1;")]
    [InlineData("csharp", "```\nnested\n```", "```\nnested\n```")]
    public void CleanTextKeepsOnlyTheCode(string? language, string code, string expected) => AssertCleanText(expected, new MarkdownCode(language, code));
}

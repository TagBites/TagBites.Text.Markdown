using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace TagBites.Text.Markdown.Tests;

/// <remarks>
/// Parses the produced Markdown back with Markdig and checks that it means what was put in.
/// </remarks>
public class ConformanceTests : MarkdownTestBase
{
    private static readonly MarkdownPipeline s_pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseTaskLists()
        .Build();

    private static readonly MarkdownPipeline s_attributePipeline = new MarkdownPipelineBuilder()
        .UseGenericAttributes()
        .Build();

    private static readonly MarkdownPipeline s_frontMatterPipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .Build();


    [Theory]
    [InlineData("# not a heading")]
    [InlineData("###### not a heading")]
    [InlineData("> not a quote")]
    [InlineData("- not an item")]
    [InlineData("+ not an item")]
    [InlineData("* not an item")]
    [InlineData("1. not an item")]
    [InlineData("99) not an item")]
    [InlineData("---")]
    [InlineData("***")]
    [InlineData("___")]
    [InlineData("Title\n===")]
    [InlineData("Title\n---")]
    [InlineData("Title\n--")]
    [InlineData("Title\n=")]
    [InlineData("===")]
    [InlineData("**not bold**")]
    [InlineData("_not italic_")]
    [InlineData("~~not strikethrough~~")]
    [InlineData("`not code`")]
    [InlineData("[not a link](x.md)")]
    [InlineData("![not an image](x.png)")]
    [InlineData("<br>")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("&amp;")]
    [InlineData("a \\ b")]
    [InlineData("Roslyn-based C# expression parser")]
    [InlineData("snake_case_name and a < b and Func<>")]
    [InlineData("version 2.0 costs 5 & 6")]
    [InlineData("issue #42 is open")]
    [InlineData("    indented by four spaces")]
    [InlineData("\tindented by a tab")]
    [InlineData("<https://tagbites.com>")]
    [InlineData("<!-- comment -->")]
    [InlineData("[foo]: /url")]
    [InlineData("[foo][bar]")]
    [InlineData("&#65;")]
    [InlineData("a\n\nb")]
    [InlineData("| Name | Value |\n| ---- | ----- |\n| a | 10 |")]
    [InlineData("Name | Value\n--- | ---")]
    [InlineData("Name | Value\n:--- | ---:")]
    [InlineData("Name | Value\n| :-: |")]
    public void ParagraphTextIsNotInterpreted(string text)
    {
        var document = new MarkdownDocument();
        document.AddParagraph(text);

        var markdown = document.ToString();
        var parsed = Markdig.Markdown.Parse(markdown, s_pipeline);

        Assert.IsType<ParagraphBlock>(Assert.Single(parsed));
        Assert.Equal(Flatten(text), Flatten(Markdig.Markdown.ToPlainText(markdown, s_pipeline)));
    }

    [Theory]
    [InlineData("plain code")]
    [InlineData("var x = 1;\nvar y = 2;")]
    [InlineData("```\nnested fence\n```")]
    [InlineData("````\nlonger fence\n````")]
    [InlineData("~~~\ntilde fence\n~~~")]
    [InlineData("a ` b")]
    [InlineData("# not a heading")]
    public void CodeBlockContentSurvivesParsing(string code)
    {
        var document = new MarkdownDocument();
        document.AddCode("csharp", code);

        var markdown = document.ToString();
        var parsed = Markdig.Markdown.Parse(markdown, s_pipeline);
        var block = Assert.IsType<FencedCodeBlock>(Assert.Single(parsed));

        Assert.Equal("csharp", block.Info);
        Assert.Equal(code, GetLines(block));
    }

    [Theory]
    [InlineData("plain")]
    [InlineData("a ` b")]
    [InlineData("`leading")]
    [InlineData("trailing`")]
    [InlineData("``double``")]
    [InlineData("```")]
    [InlineData(" spaced ")]
    [InlineData("a | b")]
    public void CodeSpanContentSurvivesParsing(string code)
    {
        var document = new MarkdownDocument();
        document.AddParagraph(MarkdownText.Code(code));

        var markdown = document.ToString();
        var parsed = Markdig.Markdown.Parse(markdown, s_pipeline);
        var paragraph = Assert.IsType<ParagraphBlock>(Assert.Single(parsed));
        var span = Assert.IsType<CodeInline>(Assert.Single(paragraph.Inline!));

        Assert.Equal(code, span.Content);
    }

    [Theory]
    [InlineData("plain")]
    [InlineData("a | b")]
    [InlineData("a\nb")]
    [InlineData("**not bold**")]
    [InlineData("[not a link](x.md)")]
    public void TableCellContentIsNotInterpreted(string cell)
    {
        var document = new MarkdownDocument();
        document.AddTable()
            .SetHeaders("head")
            .WithRow(cell);

        var markdown = document.ToString();
        var parsed = Markdig.Markdown.Parse(markdown, s_pipeline);
        var table = Assert.IsType<Markdig.Extensions.Tables.Table>(Assert.Single(parsed));
        var row = (Markdig.Extensions.Tables.TableRow)table[1];
        var content = (Markdig.Extensions.Tables.TableCell)row[0];

        Assert.Equal(Flatten(cell), Flatten(ToPlainText(content)));
    }

    [Fact]
    public void LineBreakIsReadBackAsAHardBreak()
    {
        var document = new MarkdownDocument();
        document.AddParagraph("line one" + MarkdownText.LineBreak + "line two");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var paragraph = Assert.IsType<ParagraphBlock>(Assert.Single(parsed));

        Assert.Contains(paragraph.Inline!, x => x is LineBreakInline { IsHard: true });
    }

    [Fact]
    public void AttributeAnchorNeedsTheGenericAttributesExtension()
    {
        var markdown = GetAnchoredHeader(MarkdownHeaderAnchorStyle.Attribute);

        Assert.Contains("id=\"custom-id\"", Markdig.Markdown.ToHtml(markdown, s_attributePipeline));
        Assert.Contains("{#custom-id}", Markdig.Markdown.ToHtml(markdown, s_pipeline));
    }

    [Fact]
    public void HtmlAnchorNeedsNoExtension()
    {
        var markdown = GetAnchoredHeader(MarkdownHeaderAnchorStyle.HtmlAnchor);

        Assert.Contains("id=\"custom-id\"", Markdig.Markdown.ToHtml(markdown, s_pipeline));
        Assert.Contains("id=\"custom-id\"", Markdig.Markdown.ToHtml(markdown, s_attributePipeline));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(6)]
    public void HeaderKeepsItsLevel(int level)
    {
        var document = new MarkdownDocument();
        document.AddHeader(level, "Title");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var heading = Assert.IsType<HeadingBlock>(Assert.Single(parsed));

        Assert.Equal(level, heading.Level);
    }

    [Theory]
    [InlineData(MarkdownTableColumnAlignment.Left, Markdig.Extensions.Tables.TableColumnAlign.Left)]
    [InlineData(MarkdownTableColumnAlignment.Center, Markdig.Extensions.Tables.TableColumnAlign.Center)]
    [InlineData(MarkdownTableColumnAlignment.Right, Markdig.Extensions.Tables.TableColumnAlign.Right)]
    public void ColumnAlignmentIsReadBack(MarkdownTableColumnAlignment alignment, Markdig.Extensions.Tables.TableColumnAlign expected)
    {
        var document = new MarkdownDocument();
        document.AddTable()
            .WithHeader("head", alignment)
            .WithRow("value");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var table = Assert.IsType<Markdig.Extensions.Tables.Table>(Assert.Single(parsed));

        Assert.Equal(expected, table.ColumnDefinitions[0].Alignment);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CheckListStateIsReadBack(bool isChecked)
    {
        var document = new MarkdownDocument();
        document.AddList()
            .WithCheckItem(isChecked, "task");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var list = Assert.IsType<ListBlock>(Assert.Single(parsed));
        var item = (ListItemBlock)list[0];
        var paragraph = (ParagraphBlock)item[0];
        var task = Assert.IsType<Markdig.Extensions.TaskLists.TaskList>(paragraph.Inline!.FirstChild);

        Assert.Equal(isChecked, task.Checked);
    }

    [Fact]
    public void FrontMatterIsReadBack()
    {
        var document = new MarkdownDocument
        {
            FrontMatter = new MarkdownFrontMatter { Title = "C#: the language" }
        };
        document.AddParagraph("Body.");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_frontMatterPipeline);
        var front = Assert.IsType<Markdig.Extensions.Yaml.YamlFrontMatterBlock>(parsed[0]);

        var paragraph = Assert.IsType<ParagraphBlock>(parsed[1]);

        Assert.Equal("title: \"C#: the language\"", front.Lines.Lines[0].Slice.ToString());
        Assert.Equal("Body.", ((LiteralInline)paragraph.Inline!.FirstChild!).Content.ToString());
    }

    [Fact]
    public void NestedListKeepsItsDepth()
    {
        var document = new MarkdownDocument();
        var list = document.AddList();
        var item = list.AddItem("a");

        item.AddChildItem("b")
            .AddChildItem("c");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var outer = Assert.IsType<ListBlock>(Assert.Single(parsed));
        var middle = Assert.IsType<ListBlock>(((ListItemBlock)outer[0])[1]);
        var inner = Assert.IsType<ListBlock>(((ListItemBlock)middle[0])[1]);

        Assert.Single(inner);
    }

    [Fact]
    public void QuoteKeepsItsContent()
    {
        var document = new MarkdownDocument();
        var quote = document.AddQuote("intro");

        quote.AddList()
            .WithItem("a")
            .WithItem("b");

        var parsed = Markdig.Markdown.Parse(document.ToString(), s_pipeline);
        var block = Assert.IsType<QuoteBlock>(Assert.Single(parsed));

        Assert.IsType<ParagraphBlock>(block[0]);
        Assert.IsType<ListBlock>(block[1]);
    }


    private static string GetLines(LeafBlock block)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < block.Lines.Count; i++)
        {
            if (i > 0)
                builder.Append('\n');

            builder.Append(block.Lines.Lines[i].Slice.ToString());
        }

        return builder.ToString();
    }
    private static string ToPlainText(Markdig.Extensions.Tables.TableCell cell)
    {
        var paragraph = (ParagraphBlock)cell[0];
        var builder = new StringBuilder();

        foreach (var inline in paragraph.Inline!)
            builder.Append(inline is LiteralInline literal ? literal.Content.ToString() : inline.ToString());

        return builder.ToString();
    }
    private static string Flatten(string value) => string.Join(" ", value.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));
    private static string GetAnchoredHeader(MarkdownHeaderAnchorStyle style)
    {
        var document = new MarkdownDocument();
        document.AddHeader(2, "Some section")
            .SetCustomId("custom-id");

        return document.ToString(new MarkdownFormat { HeaderAnchorStyle = style });
    }
}

namespace TagBites.Text.Markdown.Tests;

public class SectionTests : MarkdownTestBase
{
    [Fact]
    public void NestingSetsTheLevel()
    {
        var doc = new MarkdownDocument();
        var root = doc.AddSection("Title");
        root.AddParagraph("intro");

        var usage = root.AddSection("Usage");
        usage.AddParagraph("text");
        var tables = usage.AddSection("Tables");
        tables.AddParagraph("cells");

        var lists = usage.AddSection("Lists");
        lists.AddParagraph("items");

        AssertMarkdown(Lines(
            "# Title",
            "",
            "intro",
            "",
            "## Usage",
            "",
            "text",
            "",
            "### Tables",
            "",
            "cells",
            "",
            "### Lists",
            "",
            "items"), doc);
    }

    [Theory]
    [InlineData(1, "# Details")]
    [InlineData(2, "## Details")]
    [InlineData(5, "##### Details")]
    public void LevelOnAddSectionIgnoresTheNesting(int level, string expected)
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        title.AddSection(level, "Details");

        AssertMarkdown(Lines("# Title", "", expected), doc);
    }

    [Fact]
    public void LevelMayBeShallowerThanTheParent()
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        var usage = title.AddSection("Usage");
        usage.AddParagraph("text");

        var back = usage.AddSection(1, "Back");
        back.AddParagraph("more");

        AssertMarkdown(Lines(
            "# Title",
            "",
            "## Usage",
            "",
            "text",
            "",
            "# Back",
            "",
            "more"), doc);
    }

    [Fact]
    public void LevelOnAddSectionBecomesTheBaseForChildren()
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        var details = title.AddSection(3, "Details");
        details.AddSection("Members");

        AssertMarkdown(Lines(
            "# Title",
            "",
            "### Details",
            "",
            "#### Members"), doc);
    }

    [Theory]
    [InlineData(1, "# Details")]
    [InlineData(3, "### Details")]
    [InlineData(6, "###### Details")]
    public void ExplicitLevelIgnoresTheNesting(int level, string expected)
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        title.AddSection("Details").Header.Level = level;

        AssertMarkdown(Lines("# Title", "", expected), doc);
    }

    [Fact]
    public void ExplicitLevelBecomesTheBaseForChildren()
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        var details = title.AddSection("Details");
        details.Header.Level = 4;
        details.AddSection("Members");

        AssertMarkdown(Lines(
            "# Title",
            "",
            "#### Details",
            "",
            "##### Members"), doc);
    }

    [Fact]
    public void SeventhLevelBecomesBoldTextWithAHardBreak()
    {
        var doc = new MarkdownDocument();
        var section = doc.AddSection("L1");

        for (var i = 2; i <= 6; i++)
            section = section.AddSection("L" + i);

        var seventh = section.AddSection("L7");
        seventh.AddParagraph("body");

        var lines = doc.ToString().Split('\n');

        Assert.Equal("###### L6", lines[10]);
        Assert.Equal("**L7**  ", lines[12]);
        Assert.Equal("body", lines[13]);
    }

    [Fact]
    public void DeepSectionsKeepGoing()
    {
        var doc = new MarkdownDocument();
        var deep = doc.AddSection("L6");
        deep.Header.Level = 6;
        var seventh = deep.AddSection("L7");
        var eighth = seventh.AddSection("L8");
        eighth.AddParagraph("body");

        AssertMarkdown(Lines(
            "###### L6",
            "",
            "**L7**  ",
            "**L8**  ",
            "body"), doc);
    }

    [Fact]
    public void SectionCarriesAnAnchor()
    {
        var doc = new MarkdownDocument();
        doc.AddSection("Some section")
            .SetCustomId("custom-id");

        AssertMarkdown("# <a id=\"custom-id\"></a> Some section", doc);
    }

    [Fact]
    public void SectionInsideAQuoteContinuesTheLevel()
    {
        var doc = new MarkdownDocument();
        var root = doc.AddSection("Title");
        var note = root.AddQuote("note");
        note.AddSection("Inside");

        AssertMarkdown(Lines(
            "# Title",
            "",
            "> note",
            ">",
            "> ## Inside"), doc);
    }

    [Fact]
    public void SectionRenderedAloneStartsAtOne()
    {
        AssertMarkdown("# Title", new MarkdownSection("Title"));
    }

    [Fact]
    public void CleanTextKeepsOnlyTheText()
    {
        var doc = new MarkdownDocument();
        var title = doc.AddSection("Title");
        var usage = title.AddSection("Usage");
        usage.AddParagraph("text");

        AssertCleanText(Lines("Title", "", "Usage", "", "text"), doc);
    }

    [Fact]
    public void IgnoringSectionsDropsTheirContent()
    {
        var doc = new MarkdownDocument();
        doc.AddParagraph("kept");
        var title = doc.AddSection("Title");
        title.AddParagraph("dropped");

        AssertIgnoring("kept", doc, typeof(MarkdownSection));
    }

    [Fact]
    public void AddHeaderOnASectionThrows()
    {
        Assert.Throws<NotSupportedException>(() => new MarkdownSection("Title").AddHeader(2, "Nested"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void LevelOnAddSectionOutsideTheRangeThrows(int level)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MarkdownDocument().AddSection(level, "Title"));
    }
}

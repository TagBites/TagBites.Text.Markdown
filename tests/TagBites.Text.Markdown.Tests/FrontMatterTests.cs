namespace TagBites.Text.Markdown.Tests;

public class FrontMatterTests : MarkdownTestBase
{
    [Fact]
    public void PropertiesMapToLowerCaseNames()
    {
        var expected = Lines(
            "---",
            "title: Release notes",
            "description: What changed in this version.",
            "tags: [markdown, builder]",
            "weight: 10",
            "---");

        var actual = Write(x =>
        {
            x.Title = "Release notes";
            x.Description = "What changed in this version.";
            x.SetValues("tags", "markdown", "builder");
            x["weight"] = "10";
        });

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Release notes", "title: Release notes")]
    [InlineData("release-notes", "title: release-notes")]
    [InlineData("2026-08-01", "title: 2026-08-01")]
    [InlineData("C# and F#", "title: C# and F#")]
    [InlineData("", "title: \"\"")]
    [InlineData(" padded ", "title: \" padded \"")]
    [InlineData("- dash first", "title: \"- dash first\"")]
    [InlineData("#hash first", "title: \"#hash first\"")]
    [InlineData("[bracket first", "title: \"[bracket first\"")]
    [InlineData("ends with a colon:", "title: \"ends with a colon:\"")]
    [InlineData("C#: the language", "title: \"C#: the language\"")]
    [InlineData("release #42", "title: \"release #42\"")]
    [InlineData("a \"quoted\" word", "title: a \"quoted\" word")]
    [InlineData("a \\ b", "title: a \\ b")]
    [InlineData("\"quoted\" first", "title: \"\\\"quoted\\\" first\"")]
    [InlineData("path: a \\ b", "title: \"path: a \\\\ b\"")]
    [InlineData("first\nsecond", "title: \"first\\nsecond\"")]
    [InlineData("first\tsecond", "title: \"first\\tsecond\"")]
    public void ValueIsQuotedOnlyWhereItChangesTheMeaning(string value, string expected)
    {
        Assert.Equal(Lines("---", expected, "---"), Entry("title", value));
    }

    [Theory]
    [InlineData("markdown", "tags: [markdown]")]
    [InlineData("a, b", "tags: [\"a, b\"]")]
    [InlineData("[bracket]", "tags: [\"[bracket]\"]")]
    [InlineData("plain value", "tags: [plain value]")]
    public void ListItemIsQuotedOnlyWhereItChangesTheMeaning(string value, string expected)
    {
        Assert.Equal(Lines("---", expected, "---"), Write(x => x.SetValues("tags", value)));
    }

    [Fact]
    public void NameIsQuotedTheSameWayAsAValue()
    {
        Assert.Equal(Lines("---", "\"a: b\": value", "---"), Entry("a: b", "value"));
    }

    [Fact]
    public void BlockPrecedesTheContent()
    {
        var document = new MarkdownDocument { FrontMatter = new MarkdownFrontMatter { Title = "Release notes" } };
        var notes = document.AddSection("Release notes");
        notes.AddParagraph("Body.");

        var expected = Lines(
            "---",
            "title: Release notes",
            "---",
            "",
            "# Release notes",
            "",
            "Body.");

        AssertMarkdown(expected, document);
    }

    [Fact]
    public void MissingBlockWritesNothing() => AssertMarkdown("Body.", new MarkdownDocument().WithParagraph("Body."));

    [Fact]
    public void EmptyBlockWritesNothing()
    {
        var document = new MarkdownDocument { FrontMatter = new MarkdownFrontMatter() };
        document.AddParagraph("Body.");

        AssertMarkdown("Body.", document);
    }

    [Fact]
    public void EmptyListWritesNothing()
    {
        var document = new MarkdownDocument { FrontMatter = new MarkdownFrontMatter() };
        document.FrontMatter.SetValues("tags");
        document.AddParagraph("Body.");

        AssertMarkdown("Body.", document);
    }

    [Fact]
    public void PlainTextLeavesTheBlockOut()
    {
        var document = new MarkdownDocument { FrontMatter = new MarkdownFrontMatter { Title = "Release notes" } };
        document.AddParagraph("Body.");

        AssertCleanText("Body.", document);
    }

    [Fact]
    public void SettingNullRemovesTheEntry()
    {
        var header = new MarkdownFrontMatter { Title = "Release notes", ["weight"] = "10" };
        header.Title = null;

        Assert.Null(header.Title);
        Assert.Equal([Pair("weight", "10")], header);
    }

    [Fact]
    public void EntriesFollowTheOrderOfTheFirstValue()
    {
        var header = new MarkdownFrontMatter { Title = "Release notes" };
        header.SetValues("tags", "markdown");
        header["weight"] = "10";
        header.Title = "Release notes 2";

        Assert.Equal(
            [Pair("title", "Release notes 2"), Pair("tags", "markdown"), Pair("weight", "10")],
            header);
    }

    [Fact]
    public void ListEntryComesBackOncePerItem()
    {
        var header = new MarkdownFrontMatter();
        header.SetValues("tags", "markdown", "builder");

        Assert.Equal([Pair("tags", "markdown"), Pair("tags", "builder")], header);
    }

    [Fact]
    public void IndexerReachesAnEntryWithoutAProperty()
    {
        Assert.Equal(Lines("---", "weight: 10", "---"), Entry("weight", "10"));
    }

    [Fact]
    public void IndexerReadsNullForAListEntry()
    {
        var header = new MarkdownFrontMatter();
        header.SetValues("tags", "markdown");

        Assert.Null(header["tags"]);
    }

    [Fact]
    public void ValuesReadNullForAScalarEntry()
    {
        var header = new MarkdownFrontMatter { ["tags"] = "markdown" };

        Assert.Null(header.GetValues("tags"));
    }

    [Fact]
    public void ValuesReadNullForAMissingEntry() => Assert.Null(new MarkdownFrontMatter().GetValues("tags"));

    [Fact]
    public void ScalarReplacesAListEntry()
    {
        var header = new MarkdownFrontMatter();
        header.SetValues("tags", "markdown");
        header["tags"] = "builder";

        Assert.Equal("builder", header["tags"]);
        Assert.Null(header.GetValues("tags"));
    }

    [Fact]
    public void NullValuesRemoveTheEntry()
    {
        var header = new MarkdownFrontMatter { Title = "Release notes" };
        header.SetValues("tags", "markdown");
        header.SetValues("tags", null!);

        Assert.Equal([Pair("title", "Release notes")], header);
    }

    [Fact]
    public void MetadataConstructorKeepsTheOrder()
    {
        var header = new MarkdownFrontMatter([Pair("title", "Release notes"), Pair("weight", "10")]);

        Assert.Equal("Release notes", header.Title);
        Assert.Equal([Pair("title", "Release notes"), Pair("weight", "10")], header);
    }

    [Fact]
    public void MetadataConstructorBuildsAListFromARepeatedName()
    {
        var source = new MarkdownFrontMatter { Title = "Release notes" };
        source.SetValues("tags", "markdown", "builder");

        var copy = new MarkdownFrontMatter(source);

        Assert.Equal(source, copy);
        Assert.Equal(["markdown", "builder"], copy.GetValues("tags"));
    }

    [Fact]
    public void NullNameThrows() => Assert.Throws<ArgumentNullException>(() => new MarkdownFrontMatter()[null!] = "value");

    [Fact]
    public void EmptyNameThrows() => Assert.Throws<ArgumentException>(() => new MarkdownFrontMatter()[""] = "value");

    private static KeyValuePair<string, string> Pair(string name, string value) => new(name, value);
    private static string Entry(string name, string value) => Write(x => x[name] = value);
    private static string Write(Action<MarkdownFrontMatter> configure)
    {
        var document = new MarkdownDocument { FrontMatter = new MarkdownFrontMatter() };
        configure(document.FrontMatter);
        return document.ToString();
    }
}

namespace TagBites.Text.Markdown.Tests;

public class TableTests : MarkdownTestBase
{
    [Fact]
    public void ColumnAlignment()
    {
        var table = new MarkdownTable()
            .SetHeaders("col1", "c")
            .WithRow("a", "value")
            .WithRow("bb", "x");

        var expected = Lines(
            "| col1 | c     |",
            "| ---- | ----- |",
            "| a    | value |",
            "| bb   | x     |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void PipeEscaping()
    {
        var table = new MarkdownTable()
            .SetHeaders("a", "b")
            .WithRow("x|y", "z");

        var expected = Lines(
            "| a    | b |",
            "| ---- | - |",
            "| x\\|y | z |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void NewLineInCellBecomesSpace()
    {
        var table = new MarkdownTable()
            .SetHeaders("a", "b")
            .WithRow("line one\nline two", "z");

        var expected = Lines(
            "| a                 | b |",
            "| ----------------- | - |",
            "| line one line two | z |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void CarriageReturnInCellBecomesSingleSpace()
    {
        var table = new MarkdownTable()
            .SetHeaders("a")
            .WithRow("x\r\ny");

        var expected = Lines(
            "| a   |",
            "| --- |",
            "| x y |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void InlineSyntaxInCell()
    {
        var table = new MarkdownTable()
            .SetHeaders("name", "docs")
            .WithRow(MarkdownText.Bold("total"), MarkdownText.Link("guide", "x.md"));

        var expected = Lines(
            "| name      | docs          |",
            "| --------- | ------------- |",
            "| **total** | [guide](x.md) |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void ImageInCell()
    {
        var table = new MarkdownTable()
            .SetHeaders("icon")
            .WithRow(MarkdownText.Image("logo", "logo.png"));

        var expected = Lines(
            "| icon              |",
            "| ----------------- |",
            "| ![logo](logo.png) |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void MissingCells()
    {
        var table = new MarkdownTable()
            .SetHeaders("a", "b")
            .WithRow("x");

        var expected = Lines(
            "| a | b |",
            "| - | - |",
            "| x |   |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void RowsWithoutHeaders()
    {
        var table = new MarkdownTable()
            .WithRow("a", "b");

        var expected = Lines(
            "|   |   |",
            "| - | - |",
            "| a | b |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void SetHeadersReplacesHeaders()
    {
        var table = new MarkdownTable()
            .SetHeaders("old1", "old2")
            .SetHeaders("a", "b")
            .WithRow("1", "2");

        var expected = Lines(
            "| a | b |",
            "| - | - |",
            "| 1 | 2 |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void WithHeaderAppendsHeader()
    {
        var table = new MarkdownTable()
            .WithHeader("a")
            .WithHeader("b")
            .WithRow("1", "2");

        var expected = Lines(
            "| a | b |",
            "| - | - |",
            "| 1 | 2 |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void ColumnAlignments()
    {
        var table = new MarkdownTable()
            .SetHeaders("left", "center", "right", "none")
            .SetAlignments(
                MarkdownTableColumnAlignment.Left,
                MarkdownTableColumnAlignment.Center,
                MarkdownTableColumnAlignment.Right,
                MarkdownTableColumnAlignment.None)
            .WithRow("a", "b", "c", "d");

        var expected = Lines(
            "| left | center | right | none |",
            "| :--- | :----: | ----: | ---- |",
            "| a    | b      | c     | d    |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void AlignmentWidensNarrowColumn()
    {
        var table = new MarkdownTable()
            .WithHeader("a", MarkdownTableColumnAlignment.Center)
            .WithHeader("b", MarkdownTableColumnAlignment.Right)
            .WithRow("1", "2");

        var expected = Lines(
            "| a   | b  |",
            "| :-: | -: |",
            "| 1   | 2  |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void AlignmentOnlyOnSecondColumn()
    {
        var table = new MarkdownTable()
            .WithHeader("first")
            .WithHeader("second", MarkdownTableColumnAlignment.Right)
            .WithRow("a", "b");

        var expected = Lines(
            "| first | second |",
            "| ----- | -----: |",
            "| a     | b      |");

        AssertMarkdown(expected, table);
    }

    [Fact]
    public void AlignmentIsIgnoredInCleanText()
    {
        var table = new MarkdownTable()
            .SetHeaders("a", "b")
            .SetAlignments(MarkdownTableColumnAlignment.Center, MarkdownTableColumnAlignment.Right)
            .WithRow("1", "2");

        AssertCleanText(Lines("a b", "1 2"), table);
    }

    [Fact]
    public void EmptyTableRendersNothing() => AssertMarkdown(string.Empty, new MarkdownTable());

    [Fact]
    public void NullRowThrows() => Assert.Throws<ArgumentNullException>(() => new MarkdownTable().WithRow((IList<string>)null!));

    [Fact]
    public void NullAlignmentsThrows() => Assert.Throws<ArgumentNullException>(() => new MarkdownTable().SetAlignments(null!));

    [Fact]
    public void NullHeadersThrows() => Assert.Throws<ArgumentNullException>(() => new MarkdownTable().SetHeaders((string[])null!));
}

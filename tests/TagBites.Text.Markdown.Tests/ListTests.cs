namespace TagBites.Text.Markdown.Tests;

public class ListTests : MarkdownTestBase
{
    [Fact]
    public void UnorderedList()
    {
        var list = new MarkdownList()
            .WithItem("a")
            .WithItem("b");

        var expected = Lines(
            "- a",
            "- b");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void OrderedList()
    {
        var list = new MarkdownList { IsOrdered = true }
            .WithItem("a")
            .WithItem("b")
            .WithItem("c");

        var expected = Lines(
            "1. a",
            "2. b",
            "3. c");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void OrderedListStartsWhereItIsTold()
    {
        var list = new MarkdownList { IsOrdered = true, StartNumber = 8 }
            .WithItem("a")
            .WithItem("b")
            .WithItem("c");

        var expected = Lines(
            "8. a",
            "9. b",
            "10. c");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void CheckList()
    {
        var list = new MarkdownList()
            .WithCheckItem(true, "done")
            .WithCheckItem(false, "todo");

        var expected = Lines(
            "- [x] done",
            "- [ ] todo");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void NestedItems()
    {
        var list = new MarkdownList();
        list.AddItem("parent")
            .WithChildItem("child 1")
            .WithChildItem("child 2");
        list.AddItem("second");

        var expected = Lines(
            "- parent",
            "  - child 1",
            "  - child 2",
            "- second");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void NestedCheckListItems()
    {
        var list = new MarkdownList();
        list.AddCheckItem(true, "parent")
            .WithCheckChildItem(false, "sub");

        var expected = Lines(
            "- [x] parent",
            "  - [ ] sub");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void AddChildItemChainsDownwards()
    {
        var list = new MarkdownList();
        list.AddItem("a")
            .AddChildItem("b")
            .AddChildItem("c")
            .AddChildItem("d");

        var expected = Lines(
            "- a",
            "  - b",
            "    - c",
            "      - d");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void CheckBoxesNestToAnyDepth()
    {
        var list = new MarkdownList();
        list.AddCheckItem(true, "a")
            .AddCheckChildItem(false, "b")
            .AddCheckChildItem(true, "c");

        var expected = Lines(
            "- [x] a",
            "  - [ ] b",
            "    - [x] c");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void CheckedItemUnderPlainItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddCheckChildItem(true, "done");

        var expected = Lines(
            "- a",
            "  - [x] done");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void OneListHoldsItemsWithAndWithoutACheckBox()
    {
        var list = new MarkdownList();
        list.AddItem("plain");
        list.AddCheckItem(true, "done");

        AssertMarkdown(Lines("- plain", "- [x] done"), list);
    }

    [Fact]
    public void NestedListStaysTight()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        var sub = item.AddList();
        sub.AddItem("b");
        sub.AddItem("c");
        list.AddItem("second");

        var expected = Lines(
            "- a",
            "  - b",
            "  - c",
            "- second");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void NestedOrderedListStaysTight()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        var sub = item.AddList(isOrdered: true);
        sub.AddItem("first");
        sub.AddItem("second");

        var expected = Lines(
            "- a",
            "  1. first",
            "  2. second");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void ParagraphUnderItem()
    {
        var list = new MarkdownList();
        list.AddItem("a")
            .WithParagraph("details");
        list.AddItem("b");

        var expected = Lines(
            "- a",
            "",
            "  details",
            "",
            "- b");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void LooseItemsKeepTogetherWhenTheFormatSaysSo()
    {
        var list = new MarkdownList();
        list.AddItem("a")
            .WithParagraph("details");
        list.AddItem("b");

        var expected = Lines(
            "- a",
            "",
            "  details",
            "- b");

        AssertMarkdown(expected, list, new MarkdownFormat { SeparateLooseListItems = false });
    }

    [Fact]
    public void NestedItemsDoNotMakeAListLoose()
    {
        var list = new MarkdownList();
        list.AddItem("a")
            .WithChildItem("child");
        list.AddItem("b");

        AssertMarkdown(Lines("- a", "  - child", "- b"), list);
    }

    [Fact]
    public void TableUnderItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");

        item.AddTable()
            .SetHeaders("h1", "h2")
            .WithRow("x", "y");

        var expected = Lines(
            "- a",
            "",
            "  | h1 | h2 |",
            "  | -- | -- |",
            "  | x  | y  |");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void EmptyElementUnderItemLeavesNoBlankLine()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddTable();
        item.AddParagraph("kept");

        var expected = Lines(
            "- a",
            "",
            "  kept");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void CodeUnderItem()
    {
        var list = new MarkdownList();
        var item = list.AddItem("a");
        item.AddCode("csharp", "var x = 1;");

        var expected = Lines(
            "- a",
            "",
            "  ```csharp",
            "  var x = 1;",
            "  ```");

        AssertMarkdown(expected, list);
    }

    [Fact]
    public void NullItemTextBecomesEmpty() => AssertMarkdown(string.Empty, new MarkdownListItem(null!));
}

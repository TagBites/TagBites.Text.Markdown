using System.Text;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MdTable = Markdig.Extensions.Tables.Table;

namespace TagBites.Text.Markdown.Demo;

public sealed class MarkdownCodeGenerator
{
    private const string RootName = "document";
    private const int MaximumHeaderLevel = 6;
    private const int MaximumChainedTextLength = 40;

    private static readonly MarkdownPipeline s_pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseTaskLists()
        .Build();

    private readonly StringBuilder _code = new();
    private readonly List<string> _warnings = [];
    private readonly Dictionary<string, int> _names = [];
    private string _source = string.Empty;


    public MarkdownCodeGenerationResult Generate(string markdown)
    {
        _code.Clear();
        _warnings.Clear();
        _names.Clear();
        _source = markdown ?? string.Empty;

        var ast = Markdig.Markdown.Parse(_source, s_pipeline);
        var document = new MarkdownDocument();

        _code.AppendLine($"var {RootName} = new MarkdownDocument();");
        _code.AppendLine();

        WriteBlocks(ast, document, RootName, 0);

        _code.AppendLine();
        _code.AppendLine($"var markdown = {RootName}.ToString();");

        var rendered = document.ToString();
        return new MarkdownCodeGenerationResult(
            _code.ToString().TrimEnd() + Environment.NewLine,
            rendered,
            [.. _warnings],
            Normalize(rendered) == Normalize(_source));
    }

    private void WriteBlocks(IEnumerable<Block> blocks, MarkdownContentElement root, string rootName, int baseLevel)
    {
        var sections = new Stack<(int Level, MarkdownContentElement Element, string Name)>();
        var target = root;
        var targetName = rootName;
        var targetLevel = baseLevel;

        // Markdig collects link reference definitions into a group at the head of the document
        foreach (var block in blocks.OrderBy(GetSourceStart))
        {
            if (block is not HeadingBlock heading)
            {
                WriteBlock(block, target, targetName, targetLevel);
                continue;
            }

            var level = Math.Min(Math.Max(heading.Level, 1), MaximumHeaderLevel);
            if (level != heading.Level)
                _warnings.Add($"Heading level {heading.Level} is outside the range 1 to {MaximumHeaderLevel} and was clamped to {level}.");

            while (sections.Count > 0 && sections.Peek().Level >= level)
                sections.Pop();

            var parent = sections.Count > 0 ? sections.Peek() : (Level: baseLevel, Element: root, Name: rootName);
            var text = GetText(heading);

            // A quote and a list item inherit the level of the section around them, so only a level
            // that continues the nesting can be left out of the call.
            var follows = level == parent.Level + 1;

            var section = follows
                ? parent.Element.AddSection(MarkdownText.Raw(text))
                : parent.Element.AddSection(level, MarkdownText.Raw(text));

            var name = NextName("section");
            var call = follows ? $"AddSection({TextArgument(text)})" : $"AddSection({level}, {TextArgument(text)})";
            StartGroup();
            _code.AppendLine($"var {name} = {parent.Name}.{call};");

            sections.Push((level, section, name));
            target = section;
            targetName = name;
            targetLevel = level;
        }
    }
    private void WriteBlock(Block block, MarkdownContentElement target, string targetName, int level)
    {
        switch (block)
        {
            case ParagraphBlock paragraph:
                var text = GetText(paragraph);
                target.AddParagraph(MarkdownText.Raw(text));
                _code.AppendLine($"{targetName}.AddParagraph({TextArgument(text)});");
                break;

            case FencedCodeBlock fenced:
                WriteCode(fenced.Info, GetText(fenced), target, targetName);
                break;

            case CodeBlock code:
                WriteCode(null, GetText(code), target, targetName);
                break;

            case QuoteBlock quote:
                WriteQuote(quote, target, targetName, level);
                break;

            case ListBlock list:
                WriteList(list, target, targetName, level);
                break;

            case MdTable table:
                WriteTable(table, target, targetName);
                break;

            case ThematicBreakBlock:
                target.AddThematicBreak();
                _code.AppendLine($"{targetName}.AddThematicBreak();");
                break;

            case LinkReferenceDefinitionGroup group:
                WriteLinkDefinitions(group, target, targetName);
                break;

            default:
                WriteUnsupported(block, target, targetName);
                break;
        }
    }

    private void WriteCode(string? language, string text, MarkdownContentElement target, string targetName)
    {
        language = string.IsNullOrWhiteSpace(language) ? null : language!.Trim();

        if (language == null)
        {
            target.AddCode(text);
            _code.AppendLine($"{targetName}.AddCode({Literal(text, 0)});");
        }
        else
        {
            target.AddCode(language, text);
            _code.AppendLine($"{targetName}.AddCode({Literal(language, 0)}, {Literal(text, 0)});");
        }
    }
    private void WriteQuote(QuoteBlock quote, MarkdownContentElement target, string targetName, int level)
    {
        // The first paragraph becomes the quote text, the rest becomes its content
        var blocks = quote.ToList();
        var lead = blocks.Count > 0 && blocks[0] is ParagraphBlock paragraph ? paragraph : null;
        var text = lead != null ? GetText(lead) : string.Empty;

        var element = lead != null ? target.AddQuote(MarkdownText.Raw(text)) : target.AddQuote();
        var rest = lead != null ? [.. blocks.Skip(1)] : blocks;

        var call = lead != null ? $"AddQuote({TextArgument(text)})" : "AddQuote()";

        if (rest.Count == 0)
        {
            _code.AppendLine($"{targetName}.{call};");
            return;
        }

        var name = NextName("quote");
        StartGroup();
        _code.AppendLine($"var {name} = {targetName}.{call};");
        WriteBlocks(rest, element, name, level);
    }
    private void WriteList(ListBlock list, MarkdownContentElement target, string targetName, int level)
    {
        var element = list.IsOrdered ? target.AddList(true) : target.AddList();
        var creation = $"{targetName}.AddList({(list.IsOrdered ? "isOrdered: true" : string.Empty)})";
        var items = list.OfType<ListItemBlock>().ToList();

        if (TryWriteListChain(items, element, creation))
            return;

        var name = NextName(list.IsOrdered ? "orderedList" : "list");
        StartGroup();
        _code.AppendLine($"var {name} = {creation};");

        foreach (var item in items)
            WriteListItem(item, element, name, level);
    }
    private bool TryWriteListChain(List<ListItemBlock> items, MarkdownList element, string creation)
    {
        var entries = new List<(string Text, bool? IsChecked)>();

        foreach (var item in items)
        {
            var blocks = item.ToList();
            if (blocks.Count != 1 || blocks[0] is not ParagraphBlock paragraph)
                return false;

            var text = GetText(paragraph);
            if (!IsShortText(text))
                return false;

            bool? isChecked = HasTaskMarker(text) ? TryReadTaskMarker(ref text) : null;
            entries.Add((text, isChecked));
        }

        if (entries.Count == 0)
            return false;

        var calls = new List<string>();

        foreach (var (text, isChecked) in entries)
        {
            if (isChecked == null)
            {
                element.AddItem(MarkdownText.Raw(text));
                calls.Add($"WithItem({TextArgument(text)})");
            }
            else
            {
                element.AddCheckItem(isChecked.Value, MarkdownText.Raw(text));
                calls.Add($"WithCheckItem({(isChecked.Value ? "true" : "false")}, {TextArgument(text)})");
            }
        }

        WriteChain(creation, calls);
        return true;
    }
    private void WriteListItem(ListItemBlock item, MarkdownList list, string listName, int level)
    {
        var blocks = item.ToList();
        var lead = blocks.Count > 0 && blocks[0] is ParagraphBlock paragraph ? paragraph : null;
        var text = lead != null ? GetText(lead) : string.Empty;
        var rest = lead != null ? [.. blocks.Skip(1)] : blocks;

        MarkdownListItem element;
        string call;

        // A list may mix items with a check box and items without one
        if (HasTaskMarker(text))
        {
            var isChecked = TryReadTaskMarker(ref text);
            element = list.AddCheckItem(isChecked, MarkdownText.Raw(text));
            call = $"AddCheckItem({(isChecked ? "true" : "false")}, {TextArgument(text)})";
        }
        else
        {
            element = list.AddItem(MarkdownText.Raw(text));
            call = $"AddItem({TextArgument(text)})";
        }

        if (rest.Count == 0)
        {
            _code.AppendLine($"{listName}.{call};");
            return;
        }

        var name = NextName("item");
        StartGroup();
        _code.AppendLine($"var {name} = {listName}.{call};");
        WriteBlocks(rest, element, name, level);
    }
    private void WriteTable(MdTable table, MarkdownContentElement target, string targetName)
    {
        var element = target.AddTable();
        var rows = table.OfType<TableRow>().Select(x => x.OfType<TableCell>().Select(GetCellText).ToArray()).ToList();
        var chained = rows.SelectMany(x => x).All(IsShortText);
        var calls = new List<string>();

        foreach (var (row, cells) in table.OfType<TableRow>().Zip(rows, (a, b) => (Row: a, Cells: b)))
        {
            var values = cells.Select(MarkdownText.Raw).ToArray();
            var arguments = string.Join(", ", cells.Select(TextArgument));

            if (!row.IsHeader)
            {
                element.WithRow(values);
                calls.Add($"WithRow({arguments})");
                continue;
            }

            element.SetHeaders(values);
            calls.Add($"SetHeaders({arguments})");

            var alignments = GetAlignments(table, cells.Length);
            if (alignments == null)
                continue;

            element.SetAlignments(alignments);
            calls.Add(chained
                ? "SetAlignments(" + string.Join(",", alignments.Select(x => $"\n        MarkdownTableColumnAlignment.{x}")) + ")"
                : $"SetAlignments({string.Join(", ", alignments.Select(x => $"MarkdownTableColumnAlignment.{x}"))})");
        }

        if (chained && calls.Count > 0)
        {
            WriteChain($"{targetName}.AddTable()", calls);
            return;
        }

        var name = NextName("table");
        StartGroup();
        _code.AppendLine($"var {name} = {targetName}.AddTable();");

        foreach (var call in calls)
            _code.AppendLine($"{name}.{call};");
    }
    private static MarkdownTableColumnAlignment[]? GetAlignments(MdTable table, int columns)
    {
        var alignments = table.ColumnDefinitions.Take(columns).Select(x => ToAlignment(x.Alignment)).ToArray();

        return alignments.Length == 0 || alignments.All(x => x == MarkdownTableColumnAlignment.None)
            ? null
            : alignments;
    }
    private void WriteLinkDefinitions(LinkReferenceDefinitionGroup group, MarkdownContentElement target, string targetName)
    {
        // The span of the group itself covers the whole document, so use the definitions
        var start = int.MaxValue;
        var end = -1;

        foreach (var definition in group)
        {
            start = Math.Min(start, definition.Span.Start);
            end = Math.Max(end, definition.Span.End);
        }

        if (end < 0)
            return;

        var text = StripContinuationPrefixes(GetSource(new SourceSpan(start, end))).Trim();
        if (text.Length == 0)
            return;

        target.AddParagraph(MarkdownText.Raw(text));
        _code.AppendLine("// Link reference definitions, kept as text");
        _code.AppendLine($"{targetName}.AddParagraph({TextArgument(text)});");
    }
    private void WriteUnsupported(Block block, MarkdownContentElement target, string targetName)
    {
        var text = GetSource(block.Span).Trim();
        _warnings.Add($"{block.GetType().Name} has no matching element; its source was kept as a paragraph.");

        if (text.Length == 0)
            return;

        target.AddParagraph(MarkdownText.Raw(text));
        _code.AppendLine($"// {block.GetType().Name} is not part of the document model, the source is kept as text");
        _code.AppendLine($"{targetName}.AddParagraph({TextArgument(text)});");
    }

    // A chain reads better than a variable, but only while every text stays on its own line
    private void WriteChain(string creation, List<string> calls)
    {
        StartGroup();
        _code.AppendLine(creation);

        for (var i = 0; i < calls.Count; i++)
            _code.AppendLine($"    .{calls[i]}{(i == calls.Count - 1 ? ";" : string.Empty)}");

        // The statement after a chain belongs to something else, so it starts its own group
        StartGroup();
    }
    private static bool IsShortText(string text) => text.Length <= MaximumChainedTextLength && text.IndexOf('\n') < 0;

    private void StartGroup()
    {
        var separator = Environment.NewLine + Environment.NewLine;

        if (_code.Length > 0 && !EndsWith(_code, separator))
            _code.AppendLine();
    }
    private static bool EndsWith(StringBuilder builder, string value)
    {
        if (builder.Length < value.Length)
            return false;

        for (var i = 0; i < value.Length; i++)
            if (builder[builder.Length - value.Length + i] != value[i])
                return false;

        return true;
    }

    private string NextName(string prefix)
    {
        _names.TryGetValue(prefix, out var count);
        _names[prefix] = ++count;
        return count == 1 ? prefix : prefix + count;
    }
    // Markdig releases the lines of every block but a code block, so the text comes back from the span
    private string GetText(LeafBlock block)
    {
        var lines = block.Lines;
        if (lines.Count > 0)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                    builder.Append('\n');

                builder.Append(lines.Lines[i].Slice.ToString());
            }

            return builder.ToString().TrimEnd();
        }

        var text = GetSource(block.Span);

        if (block is HeadingBlock)
            text = StripHeadingMarkers(text);

        return StripContinuationPrefixes(text);
    }
    private string GetCellText(TableCell cell)
    {
        var paragraph = cell.OfType<ParagraphBlock>().FirstOrDefault();
        return paragraph != null ? GetText(paragraph).Trim() : string.Empty;
    }
    private string GetSource(SourceSpan span)
    {
        if (span.Start < 0 || span.End < span.Start || span.End >= _source.Length)
            return string.Empty;

        return _source.Substring(span.Start, span.End - span.Start + 1);
    }

    private bool IsCheckList(ListBlock list)
    {
        return !list.IsOrdered
               && list.OfType<ListItemBlock>().Any(x => x.FirstOrDefault() is ParagraphBlock paragraph && HasTaskMarker(GetText(paragraph)));
    }

    private static int GetSourceStart(Block block)
    {
        return block is LinkReferenceDefinitionGroup { Count: > 0 } group
            ? group.Min(x => x.Span.Start)
            : block.Span.Start;
    }
    private static bool HasTaskMarker(string text) => text.Length >= 3 && text[0] == '[' && text[2] == ']' && text[1] is ' ' or 'x' or 'X';
    private static string StripHeadingMarkers(string text)
    {
        var start = 0;
        while (start < text.Length && text[start] == '#')
            start++;

        while (start < text.Length && (text[start] == ' ' || text[start] == '\t'))
            start++;

        return text.Substring(start).TrimEnd().TrimEnd('#').TrimEnd();
    }
    private static string StripContinuationPrefixes(string text)
    {
        if (text.IndexOf('\n') < 0)
            return text;

        var lines = text.Split('\n');

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].TrimStart(' ', '\t');

            if (line.StartsWith(">", StringComparison.Ordinal))
            {
                line = line.Substring(1);

                if (line.StartsWith(" ", StringComparison.Ordinal))
                    line = line.Substring(1);
            }

            lines[i] = line;
        }

        return string.Join("\n", lines);
    }
    private static bool TryReadTaskMarker(ref string text)
    {
        if (text.Length < 3 || text[0] != '[' || text[2] != ']')
            return false;

        var isChecked = text[1] is 'x' or 'X';
        text = text.Substring(3).TrimStart();
        return isChecked;
    }
    private static MarkdownTableColumnAlignment ToAlignment(TableColumnAlign? align)
    {
        return align switch
        {
            TableColumnAlign.Left => MarkdownTableColumnAlignment.Left,
            TableColumnAlign.Center => MarkdownTableColumnAlignment.Center,
            TableColumnAlign.Right => MarkdownTableColumnAlignment.Right,
            _ => MarkdownTableColumnAlignment.None
        };
    }
    private static string Normalize(string value) => value.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');

    private static string TextArgument(string value)
    {
        var literal = Literal(value, 0);

        return MarkdownText.FromText(value).Markdown == value
            ? literal
            : $"MarkdownText.Raw({literal})";
    }
    private static string Literal(string value, int indent)
    {
        if (value.IndexOf('\n') < 0 && value.IndexOf('\r') < 0)
        {
            var builder = new StringBuilder("\"");

            foreach (var c in value)
                switch (c)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '"': builder.Append("\\\""); break;
                    case '\t': builder.Append("\\t"); break;
                    default: builder.Append(c); break;
                }

            return builder.Append('"').ToString();
        }

        return RawLiteral(value, indent);
    }
    private static string RawLiteral(string value, int indent)
    {
        var quotes = new string('"', Math.Max(3, LongestQuoteRun(value) + 1));
        var pad = new string(' ', indent + 4);
        var builder = new StringBuilder(quotes).Append('\n');

        foreach (var line in value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            if (line.Length > 0)
                builder.Append(pad);

            builder.Append(line).Append('\n');
        }

        return builder.Append(pad).Append(quotes).ToString();
    }
    private static int LongestQuoteRun(string value)
    {
        var longest = 0;
        var current = 0;

        foreach (var c in value)
            if (c == '"')
                longest = Math.Max(longest, ++current);
            else
                current = 0;

        return longest;
    }
}

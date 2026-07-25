namespace TagBites.Text.Markdown;

public class MarkdownTable : MarkdownElement
{
    public IList<MarkdownText> Headers { get; } = new List<MarkdownText>();
    public IList<IList<MarkdownText>> Rows { get; } = new List<IList<MarkdownText>>();
    public IList<MarkdownTableColumnAlignment> Alignments { get; } = new List<MarkdownTableColumnAlignment>();


    public MarkdownTable WithHeader(MarkdownText text)
    {
        Headers.Add(text);
        return this;
    }
    public MarkdownTable WithHeader(MarkdownText text, MarkdownTableColumnAlignment alignment)
    {
        Headers.Add(text);

        while (Alignments.Count < Headers.Count - 1)
            Alignments.Add(MarkdownTableColumnAlignment.None);

        Alignments.Add(alignment);
        return this;
    }
    public MarkdownTable SetHeaders(params MarkdownText[] columns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));

        Headers.Clear();

        foreach (var column in columns)
            Headers.Add(column);

        return this;
    }
    public MarkdownTable SetHeaders(params string[] columns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));

        Headers.Clear();

        foreach (var column in columns)
            Headers.Add(column);

        return this;
    }
    public MarkdownTable SetAlignments(params MarkdownTableColumnAlignment[] alignments)
    {
        if (alignments == null)
            throw new ArgumentNullException(nameof(alignments));

        Alignments.Clear();

        foreach (var alignment in alignments)
            Alignments.Add(alignment);

        return this;
    }

    public MarkdownTable WithRow(params MarkdownText[] textCells)
    {
        if (textCells == null)
            throw new ArgumentNullException(nameof(textCells));

        Rows.Add(textCells);
        return this;
    }
    public MarkdownTable WithRow(params string[] textCells)
    {
        if (textCells == null)
            throw new ArgumentNullException(nameof(textCells));

        return WithRow((IList<string>)textCells);
    }
    public MarkdownTable WithRow(IList<MarkdownText> textCells)
    {
        if (textCells == null)
            throw new ArgumentNullException(nameof(textCells));

        Rows.Add(textCells);
        return this;
    }
    public MarkdownTable WithRow(IList<string> textCells)
    {
        if (textCells == null)
            throw new ArgumentNullException(nameof(textCells));

        var cells = new List<MarkdownText>(textCells.Count);

        foreach (var cell in textCells)
            cells.Add(cell);

        Rows.Add(cells);
        return this;
    }

    protected internal override void Resolve(MarkdownStringBuilder builder)
    {
        if (builder.Format.IsPlainText)
            ResolveCleanText(builder);
        else
            ResolveMarkdown(builder);
    }

    private void ResolveCleanText(MarkdownStringBuilder builder)
    {
        var written = false;

        if (Headers.Count > 0)
        {
            WriteCleanTextRow(builder, Headers);
            written = true;
        }

        foreach (var row in Rows)
        {
            if (written)
                builder.AppendLine();

            WriteCleanTextRow(builder, row);
            written = true;
        }
    }
    private void ResolveMarkdown(MarkdownStringBuilder builder)
    {
        // A table without a single column has no valid delimiter row
        if (Headers.Count == 0 && Rows.Count == 0)
            return;

        var widths = GetWidths();

        WriteRow(builder, Headers, widths);
        builder.AppendLine();
        WriteSeparatorRow(builder, widths);

        foreach (var row in Rows)
        {
            builder.AppendLine();
            WriteRow(builder, row, widths);
        }
    }

    private int[] GetWidths()
    {
        var count = Headers.Count;

        foreach (var row in Rows)
            count = Math.Max(count, row.Count);

        var widths = new int[count];

        for (var i = 0; i < Headers.Count; i++)
            widths[i] = EscapeCell(Headers[i]).Length;

        foreach (var row in Rows)
            for (var i = 0; i < row.Count; i++)
                widths[i] = Math.Max(widths[i], EscapeCell(row[i]).Length);

        for (var i = 0; i < count; i++)
            widths[i] = Math.Max(widths[i], GetMinimumWidth(GetAlignment(i)));

        return widths;
    }
    private MarkdownTableColumnAlignment GetAlignment(int column) => column < Alignments.Count ? Alignments[column] : MarkdownTableColumnAlignment.None;

    private void WriteSeparatorRow(MarkdownStringBuilder builder, int[] widths)
    {
        builder.Append("| ");

        for (var i = 0; i < widths.Length; i++)
        {
            if (i > 0)
                builder.Append(" | ");

            WriteSeparator(builder, widths[i], GetAlignment(i));
        }

        builder.Append(" |");
    }

    private static void WriteRow(MarkdownStringBuilder builder, IList<MarkdownText> cells, int[] widths)
    {
        builder.Append("| ");

        for (var i = 0; i < widths.Length; i++)
        {
            if (i > 0)
                builder.Append(" | ");

            var cell = i < cells.Count ? EscapeCell(cells[i]) : string.Empty;
            builder.Append(cell);
            builder.AppendSpaces(widths[i] - cell.Length);
        }

        builder.Append(" |");
    }
    private static void WriteCleanTextRow(MarkdownStringBuilder builder, IList<MarkdownText> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            if (i > 0)
                builder.Append(" ");

            builder.Append(Flatten(cells[i].Text));
        }
    }
    private static void WriteSeparator(MarkdownStringBuilder builder, int width, MarkdownTableColumnAlignment alignment)
    {
        switch (alignment)
        {
            case MarkdownTableColumnAlignment.Left:
                builder.Append(':');
                builder.Append('-', width - 1);
                break;

            case MarkdownTableColumnAlignment.Right:
                builder.Append('-', width - 1);
                builder.Append(':');
                break;

            case MarkdownTableColumnAlignment.Center:
                builder.Append(':');
                builder.Append('-', width - 2);
                builder.Append(':');
                break;

            default:
                builder.Append('-', width);
                break;
        }
    }
    private static int GetMinimumWidth(MarkdownTableColumnAlignment alignment)
    {
        return alignment switch
        {
            MarkdownTableColumnAlignment.Center => 3,
            MarkdownTableColumnAlignment.Left or MarkdownTableColumnAlignment.Right => 2,
            _ => 1
        };
    }

    private static string EscapeCell(MarkdownText cell)
    {
        var value = Flatten(cell.Markdown);
        return value.IndexOf('|') < 0 ? value : value.Replace("|", "\\|");
    }
    // A table row is a single line, so a line break becomes a space
    private static string Flatten(string value)
    {
        return value.IndexOf('\n') < 0 && value.IndexOf('\r') < 0
            ? value
            : value.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
    }
}

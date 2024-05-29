namespace TagBites.Text.Markdown
{
    public class MarkdownTable : MarkdownElement
    {
        public List<string> Headers { get; } = new();
        public List<IList<string>> Rows { get; } = new();


        public MarkdownTable WithHeader(string text)
        {
            Headers.Add(text);
            return this;
        }
        public MarkdownTable WithHeaders(params string[] columns)
        {
            Headers.AddRange(columns);
            return this;
        }

        public MarkdownTable WithRow(params string[] textCells)
        {
            Rows.Add(textCells);
            return this;
        }
        public MarkdownTable WithRow(IList<string> textCells)
        {
            if (textCells == null)
                throw new ArgumentNullException(nameof(textCells));

            Rows.Add(textCells);
            return this;
        }

        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            builder.AppendLine();

            if (builder.CleanTextMode)
            {
                for (var i = 0; i < Rows.Count; i++)
                {
                    if (i >= 0)
                        builder.AppendLine();

                    WriteRow(builder, null, Rows[i]);
                }
            }
            else
            {
                // Widths
                var widths = new List<int>();

                foreach (var column in Headers)
                    widths.Add(column?.Length ?? 0);

                foreach (var row in Rows)
                {
                    for (var i = 0; i < row.Count; i++)
                        if (widths.Count <= i)
                            widths.Add(row[i]?.Length ?? 0);
                        else
                            widths[i] = Math.Max(widths[i], row[i]?.Length ?? 0);
                }

                for (var i = 0; i < widths.Count; i++)
                    if (widths[i] == 0)
                        widths[i] = 1;

                // Headers
                WriteRow(builder, widths, Headers);
                builder.AppendLine();

                // Line
                builder.Append("| ");
                for (var i = 0; i < widths.Count; i++)
                {
                    if (i > 0)
                        builder.Append(" | ");

                    builder.Append('-', widths[i]);
                }
                builder.Append(" |");

                // Rows
                for (var i = 0; i < Rows.Count; i++)
                {
                    if (i >= 0)
                        builder.AppendLine();

                    WriteRow(builder, widths, Rows[i]);
                }
            }
        }

        private void WriteRow(MarkdownStringBuilder builder, IList<int>? widths, IList<string> cells)
        {
            if (builder.CleanTextMode)
            {
                for (var i = 0; i < cells.Count; i++)
                {
                    if (i > 0)
                        builder.Append(" ");

                    var data = cells[i];
                    if (data != null)
                        builder.Append(data);
                }
            }
            else
            {
                builder.Append("| ");
                for (var i = 0; i < widths?.Count; i++)
                {
                    if (i > 0)
                        builder.Append(" | ");

                    var data = i < cells.Count ? cells[i] : null;
                    if (data != null)
                        builder.Append(data);

                    var spaces = widths[i] - (data?.Length ?? 0);
                    builder.AppendSpaces(spaces);
                }
                builder.Append(" |");
            }
        }
    }
}

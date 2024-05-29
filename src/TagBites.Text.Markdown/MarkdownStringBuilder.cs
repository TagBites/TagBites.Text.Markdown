using System.Text;

namespace TagBites.Text.Markdown
{
    public class MarkdownStringBuilder
    {
        private readonly Stack<int> _indents = new();

        private StringBuilder StringBuilder { get; }
        public int Indent { get; private set; }
        public int Length => StringBuilder.Length;

        public bool CleanTextMode { get; }

        public MarkdownStringBuilder(bool cleanTextMode)
            : this(new StringBuilder(), cleanTextMode)
        { }
        public MarkdownStringBuilder(StringBuilder stringBuilder, bool cleanTextMode)
        {
            StringBuilder = stringBuilder;
            CleanTextMode = cleanTextMode;
        }


        public void PushIndent() => PushIndent(Indent + 1);
        public void PushIndent(int indent)
        {
            if (indent < 0)
                throw new ArgumentOutOfRangeException(nameof(indent));

            _indents.Push(Indent);
            Indent = indent;
        }
        public void PopIndent() => Indent = _indents.Pop();

        public void Append(char value)
        {
            if (value == '\n')
                AppendLine();
            else
            {
                AppendIndent();
                StringBuilder.Append(value);
            }
        }
        public void Append(string value)
        {
            var lines = value.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    AppendLine();

                AppendIndent();
                StringBuilder.Append(lines[i]);
            }
        }
        public void Append(char value, int count)
        {
            if (value == '\n')
            {
                for (var i = 0; i < count; i++)
                    AppendLine();
            }
            else
            {
                AppendIndent();

                for (var i = 0; i < count; i++)
                    StringBuilder.Append(value);
            }
        }
        public void AppendSpaces(int count) => Append(' ', count);
        public void AppendLine()
        {
            StringBuilder.Append("\n");
        }
        private void AppendIndent()
        {
            if (StringBuilder.Length == 0 || StringBuilder[StringBuilder.Length - 1] == '\n')
                for (var i = 0; i < Indent * 4; i++)
                    StringBuilder.Append(' ');
        }

        public override string ToString() => StringBuilder.ToString();
    }
}

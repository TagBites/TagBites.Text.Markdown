namespace TagBites.Text.Markdown
{
    public class MarkdownHeader : MarkdownElement
    {
        public int Level { get; }
        public string Text { get; }

        public string? CustomId { get; set; }

        public MarkdownHeader(int level, string text)
        {
            Level = level;
            Text = text;
        }


        public MarkdownHeader WithCustomId(string customId)
        {
            CustomId = customId;
            return this;
        }

        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            if (builder.CleanTextMode)
                builder.Append(Text);
            else
            {
                for (var i = 0; i < Level; i++)
                    builder.Append('#');

                builder.Append(' ');
                builder.Append(Text);

                if (!string.IsNullOrEmpty(CustomId))
                {
                    builder.Append('{');
                    if (CustomId[0] != '#')
                        builder.Append('#');
                    builder.Append(CustomId);
                    builder.Append('}');
                }
            }
        }
    }
}

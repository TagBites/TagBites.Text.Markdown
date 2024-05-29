namespace TagBites.Text.Markdown
{
    public class MarkdownCode : MarkdownElement
    {
        public string? Language { get; }
        public string Code { get; }

        public MarkdownCode(string? language, string code)
        {
            Language = language;
            Code = code;
        }


        protected internal override void Resolve(MarkdownStringBuilder builder)
        {
            if (builder.CleanTextMode)
                builder.Append(Code);
            else
            {
                builder.Append("```");
                if (!string.IsNullOrEmpty(Language))
                    builder.Append(Language);
                builder.AppendLine();
                builder.Append(Code);
                builder.AppendLine();
                builder.Append("```");
            }
        }
    }
}

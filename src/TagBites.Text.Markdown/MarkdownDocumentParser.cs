namespace TagBites.Text.Markdown
{
    public static class MarkdownDocumentParser
    {
        private static readonly MarkdownDocumentHeader s_empty = new(new Dictionary<string, string>());


        public static MarkdownDocumentHeader ExtractHeader(ref string documentText)
        {
            if (string.IsNullOrEmpty(documentText))
                return s_empty;

            // Start '---'
            var startIndex = -1;

            if (documentText.StartsWith("---"))
            {
                for (var i = 3; i < documentText.Length; i++)
                    if (documentText[i] == '\n')
                    {
                        startIndex = i + 1;
                        break;
                    }
                    else if (!char.IsWhiteSpace(documentText[i]))
                        break;
            }

            if (startIndex < 0)
                return s_empty;

            // End '---'
            var endIndex = documentText.IndexOf("---", startIndex, StringComparison.Ordinal);
            var docStartIndex = -1;

            if (endIndex > startIndex && documentText[endIndex - 1] == '\n')
            {
                for (var i = endIndex + 3; i < documentText.Length; i++)
                    if (documentText[i] == '\n')
                    {
                        docStartIndex = i + 1;
                        break;
                    }
                    else if (!char.IsWhiteSpace(documentText[i]))
                        break;
            }

            if (docStartIndex < 0)
                return s_empty;

            // Metadata
            var headerText = documentText.Substring(startIndex, endIndex - startIndex);
            documentText = documentText.Substring(docStartIndex);

            Dictionary<string, string>? metadata = null;
            var lines = headerText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var index = line.IndexOf(':');
                if (index <= 0 || index + 1 == line.Length)
                    continue;

                var name = line.Substring(0, index).Trim();
                var value = line.Substring(index + 1).Trim();

                if (name.Length > 0 && value.Length > 0)
                {
                    if (metadata == null)
                        metadata = new Dictionary<string, string>();

                    if (!metadata.ContainsKey(name))
                        metadata.Add(name, value);
                }
            }

            return metadata == null
                ? s_empty
                : new MarkdownDocumentHeader(metadata);
        }
    }
}

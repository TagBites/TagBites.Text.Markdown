namespace TagBites.Text.Markdown;

public static class MarkdownDocumentParser
{
    public static MarkdownFrontMatter ExtractHeader(ref string documentText)
    {
        if (string.IsNullOrEmpty(documentText))
            return new MarkdownFrontMatter();

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
            return new MarkdownFrontMatter();

        // End '---'
        var endIndex = documentText.IndexOf("---", startIndex, StringComparison.Ordinal);
        var docStartIndex = -1;

        while (endIndex > startIndex && docStartIndex < 0)
        {
            if (documentText[endIndex - 1] == '\n')
            {
                var lineValid = true;
                var lineEnd = documentText.Length;

                for (var i = endIndex + 3; i < documentText.Length; i++)
                    if (documentText[i] == '\n')
                    {
                        lineEnd = i;
                        break;
                    }
                    else if (!char.IsWhiteSpace(documentText[i]))
                    {
                        lineValid = false;
                        break;
                    }

                if (lineValid)
                {
                    docStartIndex = Math.Min(lineEnd + 1, documentText.Length);
                    break;
                }
            }

            endIndex = documentText.IndexOf("---", endIndex + 3, StringComparison.Ordinal);
        }

        if (docStartIndex < 0)
            return new MarkdownFrontMatter();

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
            ? new MarkdownFrontMatter()
            : new MarkdownFrontMatter(metadata);
    }
}

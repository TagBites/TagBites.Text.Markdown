namespace TagBites.Text.Markdown;

/// <summary>
/// Specifies the syntax a document is written in.
/// </summary>
public enum MarkdownOutputKind
{
    /// <summary>
    /// Markdown.
    /// </summary>
    Markdown,
    /// <summary>
    /// Plain text, without Markdown syntax. Useful for a preview or a full text index.
    /// </summary>
    PlainText
}

namespace TagBites.Text.Markdown;

/// <summary>
/// Specifies the horizontal alignment of a table column.
/// </summary>
public enum MarkdownTableColumnAlignment
{
    /// <summary>
    /// The renderer decides. No marker is written.
    /// </summary>
    None,
    /// <summary>
    /// The column is left aligned, written as <c>:---</c>.
    /// </summary>
    Left,
    /// <summary>
    /// The column is centered, written as <c>:---:</c>.
    /// </summary>
    Center,
    /// <summary>
    /// The column is right aligned, written as <c>---:</c>.
    /// </summary>
    Right
}

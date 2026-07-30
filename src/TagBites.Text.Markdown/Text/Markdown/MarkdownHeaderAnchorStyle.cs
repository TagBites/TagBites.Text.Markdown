namespace TagBites.Text.Markdown;

/// <summary>
/// Specifies how an explicit header anchor is written.
/// </summary>
public enum MarkdownHeaderAnchorStyle
{
    /// <summary>
    /// An anchor element before the text, written as <c>&lt;a id="id"&gt;&lt;/a&gt;</c>.
    /// </summary>
    /// <remarks>Works in every renderer that allows inline HTML, including GitHub.</remarks>
    HtmlAnchor,
    /// <summary>
    /// An attribute after the text, written as <c>{#id}</c>.
    /// </summary>
    /// <remarks>
    /// Needs the Markdig <c>GenericAttributes</c> extension, which DocFX enables.
    /// Every other renderer shows the attribute as text.
    /// </remarks>
    Attribute
}

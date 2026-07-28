namespace TagBites.Text.Markdown.Demo;

public sealed class MarkdownCodeGenerationResult(string code, string renderedMarkdown, IReadOnlyList<string> warnings, bool isRoundTrip)
{
    public string Code { get; } = code;
    public string RenderedMarkdown { get; } = renderedMarkdown;
    public IReadOnlyList<string> Warnings { get; } = warnings;

    public bool IsRoundTrip { get; } = isRoundTrip;
}

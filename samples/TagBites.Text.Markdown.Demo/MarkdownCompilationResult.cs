namespace TagBites.Text.Markdown.Demo;

public sealed class MarkdownCompilationResult(MarkdownDocument? document, IReadOnlyList<string> errors)
{
    public MarkdownDocument? Document { get; } = document;
    public IReadOnlyList<string> Errors { get; } = errors;

    public bool IsSuccess => Document != null;


    public static MarkdownCompilationResult Success(MarkdownDocument document) => new(document, []);
    public static MarkdownCompilationResult Failure(IReadOnlyList<string> errors) => new(null, errors);
    public static MarkdownCompilationResult Failure(string error) => new(null, [error]);
}

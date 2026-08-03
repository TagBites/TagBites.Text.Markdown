using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TagBites.Text.Markdown.Demo;

public sealed class MarkdownCompiler(HttpClient http)
{
    private const string TypeName = "DemoDocumentBuilder";
    private const string MethodName = "Build";
    private const string DocumentVariable = "document";
    private const string BootStart = "/*json-start*/";
    private const string BootEnd = "/*json-end*/";

    private static readonly string[] s_referenceNames =
    [
        "System.Private.CoreLib",
        "System.Runtime",
        "System.Collections",
        "System.Linq",
        "netstandard",
        "TagBites.Text.Markdown"
    ];

    private static readonly string s_header = string.Join("\n",
        "using System;",
        "using System.Collections.Generic;",
        "using System.Linq;",
        "using TagBites.Text.Markdown;",
        "",
        "public static class " + TypeName,
        "{",
        "    public static MarkdownDocument " + MethodName + "()",
        "    {");

    private List<MetadataReference>? _references;


    public async Task<MarkdownCompilationResult> CompileAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return MarkdownCompilationResult.Success(new MarkdownDocument());

        var references = await GetReferencesAsync();
        var compilation = CSharpCompilation.Create(
            "DemoAssembly",
            [CSharpSyntaxTree.ParseText(Wrap(code))],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);

        if (!emit.Success)
            return MarkdownCompilationResult.Failure(GetErrors(emit.Diagnostics));

        try
        {
            var assembly = Assembly.Load(stream.ToArray());
            var method = assembly.GetType(TypeName)?.GetMethod(MethodName);

            if (method == null)
                return MarkdownCompilationResult.Failure("The snippet did not produce a document.");

            return method.Invoke(null, null) is MarkdownDocument document
                ? MarkdownCompilationResult.Success(document)
                : MarkdownCompilationResult.Failure("The snippet did not produce a document.");
        }
        catch (TargetInvocationException e)
        {
            return MarkdownCompilationResult.Failure(e.InnerException?.Message ?? e.Message);
        }
        catch (Exception e)
        {
            return MarkdownCompilationResult.Failure(e.Message);
        }
    }

    private async Task<List<MetadataReference>> GetReferencesAsync()
    {
        if (_references != null)
            return _references;

        var files = await GetFileNamesAsync();
        var references = new List<MetadataReference>(s_referenceNames.Length);

        foreach (var name in s_referenceNames)
        {
            var file = $"{name}.dll";
            if (files.TryGetValue(file, out var fingerprinted))
                file = fingerprinted;

            await using var stream = await http.GetStreamAsync($"_framework/{file}");
            using var buffer = new MemoryStream();

            await stream.CopyToAsync(buffer);
            buffer.Position = 0;

            references.Add(MetadataReference.CreateFromStream(buffer));
        }

        return _references = references;
    }

    private async Task<Dictionary<string, string>> GetFileNamesAsync()
    {
        var script = await http.GetStringAsync("_framework/dotnet.js");
        var start = script.IndexOf(BootStart, StringComparison.Ordinal);
        var end = start < 0 ? -1 : script.IndexOf(BootEnd, start, StringComparison.Ordinal);

        if (end < 0)
            return [];

        start += BootStart.Length;

        var manifest = JsonSerializer.Deserialize<BootManifest>(script.Substring(start, end - start), JsonSerializerOptions.Web);
        var files = new Dictionary<string, string>();

        foreach (var asset in (manifest?.Resources?.CoreAssembly ?? []).Concat(manifest?.Resources?.Assembly ?? []))
            files[asset.VirtualPath] = asset.Name;

        return files;
    }

    private static string Wrap(string code)
    {
        var builder = new StringBuilder(s_header);

        builder.Append('\n').Append(code).Append('\n');
        builder.Append("        return ").Append(DocumentVariable).Append(";\n");
        builder.Append("    }\n}\n");

        return builder.ToString();
    }
    private static IReadOnlyList<string> GetErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var offset = s_header.Split('\n').Length;
        var errors = new List<string>();

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
                continue;

            // The wrapper sits above the snippet, so the reported line is shifted back
            var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line - offset + 1;
            errors.Add(line > 0 ? $"line {line}: {diagnostic.GetMessage()}" : diagnostic.GetMessage());
        }

        return errors;
    }


    private sealed class BootManifest
    {
        public BootResources? Resources { get; set; }
    }
    private sealed class BootResources
    {
        public List<BootAsset>? CoreAssembly { get; set; }
        public List<BootAsset>? Assembly { get; set; }
    }
    private sealed class BootAsset
    {
        public string VirtualPath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

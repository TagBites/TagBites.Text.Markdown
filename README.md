# TagBites.Text.Markdown

[![Nuget](https://img.shields.io/nuget/v/TagBites.Text.Markdown.svg)](https://www.nuget.org/packages/TagBites.Text.Markdown/)
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4)
[![License](https://img.shields.io/github/license/TagBites/TagBites.Text.Markdown)](https://github.com/TagBites/TagBites.Text.Markdown/blob/master/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/TagBites.Text.Markdown.svg)](https://www.nuget.org/packages/TagBites.Text.Markdown/)

**TagBites.Text.Markdown is a C# library for programmatically building Markdown documents.** A document is a tree of typed elements. The generated output follows CommonMark, supports GitHub Flavored Markdown tables, task lists, and some Markdig extensions.

[Try it online](https://tagbites.com/markdown/demo/) - paste Markdown and read the C# code that rebuilds it.

## Install

```
dotnet add package TagBites.Text.Markdown
```

Targets `netstandard2.0`. No dependencies.

## Usage

```csharp
var doc = new MarkdownDocument();
doc.AddHeader(1, "TagBites.Expressions");
doc.AddHeader(2, "Options");

doc.AddParagraph("Every option is set on ExpressionParserOptions.");

doc.AddTable()
    .SetHeaders("Option", "Purpose")
    .WithRow("Parameters", "names the inputs")
    .WithRow("StaticImports", "acts like using static");

doc.AddParagraph("What the parser accepts:");

doc.AddList()
    .WithCheckItem(true, "operators and precedence")
    .WithCheckItem(true, "pattern matching and tuples")
    .WithCheckItem(true, "lambdas and LINQ")
    .WithCheckItem(false, "statements");

var markdown = doc.ToString();
```

Output:

```markdown
# TagBites.Expressions

## Options

Every option is set on ExpressionParserOptions.

| Option        | Purpose                |
| ------------- | ---------------------- |
| Parameters    | names the inputs       |
| StaticImports | acts like using static |

What the parser accepts:

- [x] operators and precedence
- [x] pattern matching and tuples
- [x] lambdas and LINQ
- [ ] statements
```

## Elements

- headers (`AddHeader`), with optional custom id
- paragraphs (`AddParagraph`)
- code blocks (`AddCode`), with optional language
- quotes (`AddQuote`), multiline and nestable
- unordered lists (`AddList`)
- ordered lists (`AddList(isOrdered: true)`)
- task lists (`AddCheckItem`), a check box on any list item
- tables (`AddTable`), with padded columns, column alignment and cell escaping
- thematic breaks (`AddThematicBreak`)

`MarkdownDocument`, `MarkdownSection`, `MarkdownQuote` and `MarkdownListItem` hold any block element. `MarkdownList` holds items and `MarkdownTable` holds cells. Every other element is a leaf.

Method prefixes:

| Prefix  | Effect                | Returns         |
| ------- | --------------------- | --------------- |
| `Add*`  | Appends a new element | The new element |
| `With*` | Appends a new element | The same object |
| `Set*`  | Replaces a value      | The same object |

`Add*` goes one level deeper, `With*` and `Set*` stay put, so a whole document fits in one expression:

```csharp
var doc = new MarkdownDocument()
    .WithHeader(1, "Title")
    .WithParagraph("Intro.")
    .WithElement(new MarkdownList()
        .WithItem("a")
        .WithItem("b"));
```

> There is no `WithList` or `WithTable`, because it would produce an empty element. So build those first and pass them as argument to `WithElement`.

## Sections

A section is a header plus everything under it, and the level comes from the nesting:

```csharp
var root = doc.AddSection("TagBites.Text.Markdown");
root.AddParagraph("C# library for building Markdown.");

var usage = root.AddSection("Usage");
usage.AddParagraph("Install it and start.");

var tables = usage.AddSection("Tables");
tables.AddParagraph("...");
```

Output:

```markdown
# TagBites.Text.Markdown

C# library for building Markdown.

## Usage

Install it and start.

### Tables

...
```

> A section writes its own header, so `AddHeader` on a section throws - nest another section instead.

A level can be forced using an overload:

```csharp
parent.AddSection(3, "Details");
```

**Past level six Markdown has no header**, and a deeper section falls back to bold text with a hard line break:

```markdown
###### Level six

**Level seven**  
Content of the seventh level.
```

An explicit anchor comes from `SetCustomId`:

```csharp
section.SetCustomId("custom-id"); // ## <a id="custom-id"></a> Some section
```

`MarkdownFormat.HeaderAnchorStyle` switches that to `{#custom-id}`.

## Text and escaping

Every element takes a `MarkdownText`. A `string` you pass converts implicitly and is escaped, so text from an untrusted source cannot introduce markup:

```csharp
doc.AddParagraph("Report by [admin](https://link.example) **now**");
// Report by \[admin\](https://link.example) \*\*now\*\*
```

Escaping is minimal. A character is escaped where it would change the parse and left alone where it would not:

```csharp
doc.AddParagraph("TagBites.Expressions accepts digit separators like 1_000_000 and compiles to Func<>");
// TagBites.Expressions accepts digit separators like 1_000_000 and compiles to Func<>
```

Content that is already Markdown goes through `MarkdownText.Raw`. The inline builders return raw content too:

```csharp
MarkdownText.Bold("text");                    // **text**
MarkdownText.Italic("text");                  // _text_
MarkdownText.Strikethrough("text");           // ~~text~~
MarkdownText.Code("var x;");                  // `var x;`
MarkdownText.Link("name", "https://x.com");   // [name](https://x.com)
MarkdownText.Image("logo", "logo.png");       // ![logo](logo.png)
MarkdownText.LineBreak;                       // two spaces and a new line
```

Combine with `+`:

```csharp
var text = MarkdownText.Bold("total") + " for [all] items";
// text.Markdown -> **total** for \[all\] items
// text.Text     -> total for [all] items
```

The plain text mode returns `Text`.

## Tables

A cell holds inline content, so bold text, links and images go in as text:

```csharp
table.SetHeaders("name", "docs")
    .WithRow(MarkdownText.Bold("total"), MarkdownText.Link("guide", "x.md"));
```

```markdown
| name      | docs          |
| --------- | ------------- |
| **total** | [guide](x.md) |
```

Alignment comes from `SetAlignments`, or from `WithHeader` one column at a time:

```csharp
table.SetHeaders("left", "center", "right")
    .SetAlignments(
        MarkdownTableColumnAlignment.Left,
        MarkdownTableColumnAlignment.Center,
        MarkdownTableColumnAlignment.Right)
    .WithRow("a", "b", "c");
```

```markdown
| left | center | right |
| :--- | :----: | ----: |
| a    | b      | c     |
```

## Format

Rendering options live on `MarkdownFormat`:

|         Property         |                                        Meaning                                         |
| ------------------------ | -------------------------------------------------------------------------------------- |
| `Output`                 | `Markdown` or `PlainText`.                                                             |
| `IgnoredElementTypes`    | Element types (including derived) left out of the output, together with their content. |
| `HeaderAnchorStyle`      | `HtmlAnchor` for `<a id="id"></a>`, `Attribute` for `{#id}`.                           |
| `SeparateLooseListItems` | Whether a blank line separates the items of a loose list.                              |

Whole element types can be left out, which gives a description without the code that goes with it:

```csharp
var format = new MarkdownFormat
{
    Output = MarkdownOutputKind.PlainText,
    IgnoredElementTypes = { typeof(MarkdownCode) }
};

doc.ToString(format);
```

Plain text output strips the syntax: headers, quotes and code blocks keep their text, lists lose their markers, tables come out as space-separated rows. A checkbox outputs as `☑` or `☐`. Ignoring `MarkdownCode` removes code blocks and keeps a code span inside a sentence.

```csharp
var plain = MarkdownFormat.PlainText;

new MarkdownHeader(1, "Title").ToString(plain);                     // Title
new MarkdownCode("csharp", "var x;").ToString(plain);               // var x;
new MarkdownListItem("task") { IsChecked = true }.ToString(plain);  // ☑ task
```

The format freezes the first time it is used for writing. A later change throws `InvalidOperationException`.

## Front matter

```csharp
var doc = new MarkdownDocument
{
    FrontMatter = new MarkdownFrontMatter
    {
        Title = "Release notes",
        Description = "What changed in this version.",
        ["date"] = "2026-08-01"
    }
};

doc.FrontMatter.SetValues("tags", "markdown", "builder");

var notes = doc.AddSection("Release notes");
notes.AddParagraph("First public version.");
```

Output:

```markdown
---
title: Release notes
description: What changed in this version.
date: 2026-08-01
tags: [markdown, builder]
---

# Release notes

First public version.
```

## Standards

The output follows [CommonMark](https://spec.commonmark.org/) and the [GitHub Flavored Markdown](https://github.github.com/gfm/) extensions the model exposes: tables, task lists and strikethrough. Every construct is parsed back with Markdig in the test suite and has to produce the same document.

## Limitations

- The library builds Markdown, it does not parse it. If you need to read Markdown, use [Markdig](https://github.com/xoofx/markdig).
- Escaping keeps text inside its block. A backslash cannot escape white space, so leading indentation and a blank line come out as the `&#32;` entity instead.
- Table cells hold inline content only, which is all the [GitHub Flavored Markdown spec](https://github.github.com/gfm/#tables-extension-) allows.

# TagBites.Text.Markdown

[![Nuget](https://img.shields.io/nuget/v/TagBites.Text.Markdown.svg)](https://www.nuget.org/packages/TagBites.Text.Markdown/)
[![License](https://img.shields.io/github/license/TagBites/TagBites.Text.Markdown)](https://github.com/TagBites/TagBites.Text.Markdown/blob/master/LICENSE)

C# library for programmatically building Markdown documents. Easily add headers, tables, checklists, and more with a simple, object-oriented API.

Supported elements:
- headers
- paragraphs
- code blocks
- quotes
- unordered lists
- ordered lists
- checklists
- tables

Inline syntax generated by `MarkdownSyntax`:
- bold
- italic
- strike
- code
- link
- image
- html escape

## Example

```csharp
var doc = new MarkdownDocument();
doc.AddHeader(1, "My Document");
doc.AddHeader(2, "Some section");

doc.AddParagraph("Some table below.");

var table = doc.AddTable();
table.WithHeaders("col1", "col2", "col3");
table.WithRow("a", "b", "c");
table.WithRow("1", "2", "3");

doc.AddParagraph("Some check list below.");

var list = doc.AddCheckList();
list.AddElement(true, "task 1");
list.AddElement(true, "task 2");
list.AddElement(false, "task 3");
list.AddElement(false, "task 4");
```

Output:
```markdown
# My Document

## Some section

Some table below.

| col1 | col2 | col3 |
| ---- | ---- | ---- |
| a    | b    | c    |
| 1    | 2    | 3    |

Some check list below.

- [x] task 1
- [x] task 2
- [ ] task 3
- [ ] task 4
```

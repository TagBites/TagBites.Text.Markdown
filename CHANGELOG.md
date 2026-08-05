# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2026-08-05

### Added

- `MarkdownText.Link` takes a header or a section and derives the address from its text, so a cross reference needs no hand-written anchor.
- `MarkdownText.Link` and `MarkdownText.Image` take a title, which a renderer shows as a tooltip.
- `MarkdownList.StartNumber` sets the number the first item of an ordered list carries.

### Fixed

- A link no longer breaks when its address holds a space or a parenthesis.

## [2.0.0] - 2026-08-03

### Added

- Text is escaped by default, because every element takes a `MarkdownText` and a `string` converts to it implicitly. Escaping stays minimal, so `Roslyn-based C# parser` and `1_000_000` pass through as written.
- `MarkdownText.Raw` keeps content that is already Markdown, and `+` joins raw and escaped content.
- `MarkdownSection` pairs a header with the content below it and takes its level from the nesting, so a document carries no hand-written numbers.
- `MarkdownFormat` replaces the boolean of `ToString(true)` and holds the choices that depend on the renderer: plain text output, element types to leave out, the form of a header anchor and the spacing of a loose list.
- `MarkdownDocument.FrontMatter` writes a YAML front matter block above the content, with `Title` and `Description` as properties and an indexer for every other entry.
- A quote and a list item hold block elements, so a list, a table or a header goes inside one. A quote used to hold a single piece of text.
- `MarkdownTable` writes column alignment through `SetAlignments` and `WithHeader`.
- `MarkdownText.LineBreak` ends a line without ending the block, and `AddThematicBreak` writes a horizontal rule.
- The output is formatted: one blank line between blocks, table columns padded to an equal width and the content of a list item indented to its text column.

### Changed

- Many members are renamed to say what they do, so `Add` adds and returns the new element, `With` adds and returns the receiver, and `Set` replaces a value. The six `With` overloads that took a built element become one `WithElement`.
- A check box is a property of the item, `MarkdownListItem.IsChecked`, and numbering a property of the list, `MarkdownList.IsOrdered`, so `MarkdownCheckList`, `MarkdownCheckListItem` and `MarkdownOrderedList` are gone.
- The inline builders moved from `MarkdownSyntax` to `MarkdownText` and return `MarkdownText` instead of `string`, so a fragment is never escaped twice.
- Passing `null` as content produces an empty element instead of `ArgumentNullException`.
- `MarkdownHeader` rejects a level outside the range one to six with `ArgumentOutOfRangeException`.

### Removed

- `MarkdownSyntax` is removed, together with its `EscapeHtml`, which text escaping makes unnecessary.
- `MarkdownDocumentParser` is removed. Markdig extracts a front matter block with `UseYamlFrontMatter`.

### Fixed

- A code block fence and a code span delimiter outlast their content, which used to close the block or the span early.
- A table cell escapes `|` and writes a line break as a space, both of which used to break the row structure.
- A nested list item renders as a list item. The child text used to be written with an indent but without the `-` marker.
- A quote prefixes every line with `>`, so a multi-line quote no longer ends after its first line.

## [1.0.0] - 2024-05-29

First release. Document model with headers, paragraphs, code blocks, quotes, lists, ordered lists, checklists and tables. Inline syntax through `MarkdownSyntax`, plain text rendering through `ToString(true)` and front matter extraction through `MarkdownDocumentParser`.

[2.1.0]: https://github.com/TagBites/TagBites.Text.Markdown/compare/2.0.0...2.1.0
[2.0.0]: https://github.com/TagBites/TagBites.Text.Markdown/compare/1.0.0...2.0.0
[1.0.0]: https://github.com/TagBites/TagBites.Text.Markdown/releases/tag/1.0.0

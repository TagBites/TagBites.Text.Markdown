# Security Policy

## Supported versions

| Version | Supported |
| ------- | --------- |
| 2.0.x   | Yes       |
| < 2.0   | No        |

Security fixes go into the latest released minor version. Older versions receive no updates.

## Reporting a vulnerability

Report a vulnerability privately through [GitHub security advisories](https://github.com/TagBites/TagBites.Text.Markdown/security/advisories/new). Do not use a public issue for a security report.

Include the affected version, a description of the problem and the steps to reproduce it. The maintainers answer within a few business days.

## Security model

The library turns caller-supplied strings into a Markdown document. It reads no files, opens no connections, loads no assemblies and deserializes nothing. The whole attack surface is the text that the caller passes in and the text that `ToString` returns.

### Text is escaped, raw content is not

Every element that takes content takes a `MarkdownText`. A `string` converts implicitly and is escaped, so text from an untrusted source cannot introduce markup: no link, no image that triggers a request when the document is rendered, no fenced code block that ends early, and no raw HTML such as `<script>`.

`MarkdownText.Raw` turns escaping off for a piece of content, and the inline builders on `MarkdownText` return raw content by design. Both are the point where an untrusted value becomes dangerous:

- Never pass untrusted text to `MarkdownText.Raw` or to `AddHtml`, which both reach the output unchanged.
- Never build a link or an image address from untrusted text. `MarkdownText.Link` and `MarkdownText.Image` escape the display text but emit the address unchanged, so an address such as `javascript:...` reaches the renderer.
- The content of a code block is emitted unchanged, because escaping would alter the code. A fence inside untrusted code can end the block early.

Escaping is minimal by design: a character is escaped where it would change the parse and left alone where it would not. This keeps prose readable, and it does not weaken the guarantee, because the decision is made per position rather than per character.

Structure is protected the same way. A table cell escapes `|` and writes a line break as a space, so an untrusted cell cannot add a column or end the row. `MarkdownHeader.CustomId` rejects a brace, a quote, an angle bracket and white space with `ArgumentException`, so an anchor cannot escape the attribute it is written into.

### Rendering is still the reader's trust boundary

Escaped text is safe, but a document also carries whatever the caller passed as raw content or as a link address. Render a document that mixes trusted and untrusted parts with a renderer configured to disable raw HTML, or sanitize the produced HTML.

### Front matter values cannot open another entry

`MarkdownFrontMatter` writes a value in the quoted form wherever a plain one would change the meaning, so a value carrying `:`, `#` or a line break stays a single entry. The block is data for the tool that reads it, and its values carry the same risk as any other untrusted string when that tool passes them on.

## Out of scope

The following cases are not treated as vulnerabilities in this library.

- Markup or raw HTML in the output that the caller supplied through `MarkdownText.Raw`, `AddHtml`, an inline builder or a code block.
- A bare URL in the text that a renderer turns into a link. Linking a literal URL is what GitHub Flavored Markdown does with text, and the produced Markdown carries no link syntax.
- A link or an image address that the caller built from an untrusted value.
- Behavior of the renderer that consumes the produced Markdown.
- Memory use of a document that the caller built from very large input.

using System.Text;

namespace TagBites.Text.Markdown;

// A character is escaped only where it carries syntax at that position, so ordinary prose stays readable
internal static class MarkdownEscaper
{
    private const int MaxEntityLength = 32;
    private const int TabWidth = 4;


    public static string Escape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text.IndexOf('\n') < 0)
            return EscapeLine(text, true);

        var lines = text.Split('\n');

        for (var i = 0; i < lines.Length; i++)
            lines[i] = EscapeLine(lines[i], i == 0);

        return string.Join("\n", lines);
    }

    public static int GetLongestRun(string text, char value)
    {
        var longest = 0;
        var current = 0;

        foreach (var c in text)
            if (c == value)
                longest = Math.Max(longest, ++current);
            else
                current = 0;

        return longest;
    }

    private static string EscapeLine(string line, bool isFirstLine)
    {
        // A blank line would end the block, so it is filled with an entity
        if (!isFirstLine && IsBlank(line))
            return "&#32;" + line;

        var markerIndex = GetBlockMarkerIndex(line, isFirstLine);
        var indentIndex = isFirstLine && OpensIndentedCode(line) ? 0 : -1;
        StringBuilder? builder = null;

        for (var i = 0; i < line.Length; i++)
        {
            // A space carries no backslash escape, so the indent is broken with an entity
            if (i == indentIndex)
            {
                builder = new StringBuilder(line.Length + 8);
                builder.Append(line[i] == '\t' ? "&#9;" : "&#32;");
                continue;
            }

            if (i != markerIndex && !NeedsInlineEscape(line, i))
            {
                builder?.Append(line[i]);
                continue;
            }

            if (builder == null)
            {
                builder = new StringBuilder(line.Length + 8);
                builder.Append(line, 0, i);
            }

            builder.Append('\\');
            builder.Append(line[i]);
        }

        return builder?.ToString() ?? line;
    }

    private static bool IsBlank(string line)
    {
        foreach (var c in line)
            if (!char.IsWhiteSpace(c))
                return false;

        return true;
    }
    // Four columns of leading white space open an indented code block
    private static bool OpensIndentedCode(string line)
    {
        var columns = 0;

        foreach (var c in line)
        {
            if (c == ' ')
                columns++;
            else if (c == '\t')
                columns += TabWidth - columns % TabWidth;
            else
                break;

            if (columns >= TabWidth)
                return true;
        }

        return false;
    }

    // Returns -1 when the line opens no block
    private static int GetBlockMarkerIndex(string line, bool isFirstLine)
    {
        var start = 0;
        while (start < line.Length && (line[start] == ' ' || line[start] == '\t'))
            start++;

        if (start == line.Length)
            return -1;

        var c = line[start];

        // Blockquote
        if (c == '>')
            return start;

        // Setext heading underline, which turns the line above into a heading
        if (!isFirstLine && (c == '=' || c == '-') && IsRunOf(line, start, c, 1))
            return start;

        // Table delimiter row, which turns the line above into a header row
        if (!isFirstLine && (c == '|' || c == '-' || c == ':'))
        {
            var bar = GetTableDelimiterBarIndex(line, start);
            if (bar >= 0)
                return bar;
        }

        // Thematic break
        if ((c == '-' || c == '*' || c == '_') && IsRunOf(line, start, c, 3))
            return start;

        // Bullet list
        if ((c == '-' || c == '+' || c == '*') && IsFollowedBySpaceOrEnd(line, start))
            return start;

        // Heading
        if (c == '#')
        {
            var end = start;
            while (end < line.Length && line[end] == '#')
                end++;

            if (end - start <= 6 && IsFollowedBySpaceOrEnd(line, end - 1))
                return start;
        }

        // Ordered list
        if (c >= '0' && c <= '9')
        {
            var end = start;
            while (end < line.Length && line[end] >= '0' && line[end] <= '9')
                end++;

            if (end - start <= 9 && end < line.Length && (line[end] == '.' || line[end] == ')') && IsFollowedBySpaceOrEnd(line, end))
                return end;
        }

        return -1;
    }
    // A cell holds hyphens with an optional colon on either side, and the outer bars are optional.
    // The bar is what gets escaped, because a backslash in front of a hyphen leaves the row valid.
    private static int GetTableDelimiterBarIndex(string line, int start)
    {
        var index = start;
        var bar = -1;
        var cells = 0;

        if (line[index] == '|')
        {
            bar = index;
            index++;
        }

        while (true)
        {
            index = SkipSpaces(line, index);

            if (index < line.Length && line[index] == ':')
                index++;

            var hyphens = 0;
            while (index < line.Length && line[index] == '-')
            {
                hyphens++;
                index++;
            }

            if (hyphens == 0)
                return -1;

            if (index < line.Length && line[index] == ':')
                index++;

            cells++;
            index = SkipSpaces(line, index);

            if (index == line.Length)
                break;

            if (line[index] != '|')
                return -1;

            if (bar < 0)
                bar = index;

            index = SkipSpaces(line, index + 1);

            if (index == line.Length)
                break;
        }

        return cells > 0 ? bar : -1;
    }
    private static int SkipSpaces(string line, int index)
    {
        while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
            index++;

        return index;
    }
    // A run may be broken by spaces, which a thematic break allows
    private static bool IsRunOf(string line, int start, char marker, int minimumCount)
    {
        var count = 0;

        for (var i = start; i < line.Length; i++)
        {
            var c = line[i];

            if (c == marker)
                count++;
            else if (c != ' ' && c != '\t')
                return false;
        }

        return count >= minimumCount;
    }
    private static bool IsFollowedBySpaceOrEnd(string line, int index)
    {
        var next = index + 1;
        return next == line.Length || line[next] == ' ' || line[next] == '\t';
    }

    private static bool NeedsInlineEscape(string line, int index)
    {
        switch (line[index])
        {
            case '\\':
            case '`':
            case '[':
            case ']':
            case '*':
                return true;

            // Emphasis needs a word boundary, so snake_case stays intact
            case '_':
                return !(IsWordCharacter(line, index - 1) && IsWordCharacter(line, index + 1));

            // Only a tag or an autolink, not a comparison and not Func<>
            case '<':
                return index + 1 < line.Length && (char.IsLetter(line[index + 1]) || line[index + 1] is '/' or '!' or '?');

            case '&':
                return IsEntity(line, index);

            case '!':
                return index + 1 < line.Length && line[index + 1] == '[';

            case '~':
                return index + 1 < line.Length && line[index + 1] == '~';

            default:
                return false;
        }
    }
    private static bool IsWordCharacter(string line, int index) => index >= 0 && index < line.Length && char.IsLetterOrDigit(line[index]);
    private static bool IsEntity(string line, int index)
    {
        var i = index + 1;
        if (i < line.Length && line[i] == '#')
            i++;

        var start = i;
        var end = Math.Min(line.Length, index + MaxEntityLength);

        while (i < end && char.IsLetterOrDigit(line[i]))
            i++;

        return i > start && i < line.Length && line[i] == ';';
    }
}

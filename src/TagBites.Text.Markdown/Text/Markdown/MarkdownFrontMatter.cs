using System.Globalization;
using System.Text;

namespace TagBites.Text.Markdown;

public class MarkdownFrontMatter : IEnumerable<KeyValuePair<string, string>>
{
    private const string Indicators = "-?:,[]{}#&*!|>'\"%@`";

    private readonly List<Entry> _entries = [];

    public string? Title { get => this["title"]; set => this["title"] = value; }
    public string? Description { get => this["description"]; set => this["description"] = value; }

    internal bool IsEmpty
    {
        get
        {
            foreach (var entry in _entries)
                if (entry.HasValue)
                    return false;

            return true;
        }
    }

    public string? this[string name]
    {
        get
        {
            ValidateName(name);

            var index = IndexOf(name);
            return index < 0 ? null : _entries[index].Value;
        }
        set
        {
            ValidateName(name);

            var index = IndexOf(name);

            if (value == null)
            {
                if (index >= 0)
                    _entries.RemoveAt(index);

                return;
            }

            if (index < 0)
                _entries.Add(new Entry(name) { Value = value });
            else
            {
                _entries[index].Value = value;
                _entries[index].Values = null;
            }
        }
    }

    public MarkdownFrontMatter() { }
    public MarkdownFrontMatter(IEnumerable<KeyValuePair<string, string>> entries)
    {
        if (entries == null)
            throw new ArgumentNullException(nameof(entries));

        foreach (var pair in entries)
            Add(pair.Key, pair.Value);
    }


    public IReadOnlyList<string>? GetValues(string name)
    {
        ValidateName(name);

        var index = IndexOf(name);
        return index < 0 ? null : _entries[index].Values;
    }
    public MarkdownFrontMatter SetValues(string name, params string[] values)
    {
        ValidateName(name);

        var index = IndexOf(name);

        if (values == null)
        {
            if (index >= 0)
                _entries.RemoveAt(index);

            return this;
        }

        var entry = index < 0 ? null : _entries[index];

        if (entry == null)
        {
            entry = new Entry(name);
            _entries.Add(entry);
        }

        entry.Value = null;
        entry.Values = [.. values];
        return this;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var entry in _entries)
        {
            if (entry.Value != null)
            {
                yield return new KeyValuePair<string, string>(entry.Name, entry.Value);
                continue;
            }

            if (entry.Values == null)
                continue;

            foreach (var value in entry.Values)
                yield return new KeyValuePair<string, string>(entry.Name, value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Resolve(MarkdownStringBuilder builder)
    {
        if (IsEmpty)
            return;

        builder.Append("---");

        foreach (var entry in _entries)
        {
            if (!entry.HasValue)
                continue;

            builder.AppendLine();
            AppendScalar(builder, entry.Name, false);
            builder.Append(": ");

            if (entry.Value != null)
                AppendScalar(builder, entry.Value, false);
            else
                AppendList(builder, entry.Values!);
        }

        builder.AppendLine();
        builder.Append("---");
    }

    private void Add(string name, string value)
    {
        if (value == null)
            return;

        ValidateName(name);

        var index = IndexOf(name);

        if (index < 0)
        {
            _entries.Add(new Entry(name) { Value = value });
            return;
        }

        var entry = _entries[index];

        if (entry.Values == null)
        {
            entry.Values = [];

            if (entry.Value != null)
                entry.Values.Add(entry.Value);

            entry.Value = null;
        }

        entry.Values.Add(value);
    }
    private int IndexOf(string name)
    {
        for (var i = 0; i < _entries.Count; i++)
            if (string.Equals(_entries[i].Name, name, StringComparison.Ordinal))
                return i;

        return -1;
    }

    private static void ValidateName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (name.Length == 0)
            throw new ArgumentException("Entry name can not be empty.", nameof(name));
    }
    private static void AppendList(MarkdownStringBuilder builder, List<string> values)
    {
        builder.Append('[');

        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");

            AppendScalar(builder, values[i] ?? string.Empty, true);
        }

        builder.Append(']');
    }
    private static void AppendScalar(MarkdownStringBuilder builder, string value, bool inList)
    {
        if (!NeedsQuoting(value, inList))
        {
            builder.Append(value);
            return;
        }

        var text = new StringBuilder(value.Length + 2);
        text.Append('"');

        foreach (var c in value)
            switch (c)
            {
                case '\\': text.Append("\\\\"); break;
                case '"': text.Append("\\\""); break;
                case '\n': text.Append("\\n"); break;
                case '\r': text.Append("\\r"); break;
                case '\t': text.Append("\\t"); break;

                default:
                    if (c < ' ' || c == '\u007f')
                        text.Append("\\x").Append(((int)c).ToString("x2", CultureInfo.InvariantCulture));
                    else
                        text.Append(c);
                    break;
            }

        text.Append('"');
        builder.Append(text.ToString());
    }
    private static bool NeedsQuoting(string value, bool inList)
    {
        if (value.Length == 0)
            return true;

        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]))
            return true;

        if (Indicators.IndexOf(value[0]) >= 0 || value[value.Length - 1] == ':')
            return true;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c < ' ' || c == '\u007f')
                return true;

            // A comment opens after a space and a mapping ends before one
            if (c == '#' && i > 0 && value[i - 1] == ' ' || c == ':' && i + 1 < value.Length && value[i + 1] == ' ')
                return true;

            if (inList && c is ',' or '[' or ']' or '{' or '}')
                return true;
        }

        return false;
    }


    private sealed class Entry(string name)
    {
        public string Name { get; } = name;
        public string? Value { get; set; }
        public List<string>? Values { get; set; }

        public bool HasValue => Value != null || Values is { Count: > 0 };
    }
}

using System.IO;
using System.Text;

namespace TagBites.Text.Markdown;

internal class MarkdownStreamRenderer : MarkdownRenderer
{
    private const int WindowLength = 64;
    private const int SendThreshold = 4096;

    private readonly TextWriter _writer;
    private readonly StringBuilder _window = new();
    private int _sent;

    public override int Length => _sent + _window.Length;

    public MarkdownStreamRenderer(TextWriter writer, MarkdownFormat format)
        : base(format)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }


    public override void Flush()
    {
        Send(_window.Length);
        _writer.Flush();
    }

    protected override void Write(char value) => _window.Append(value);
    protected override void Write(string value) => _window.Append(value);
    protected override char TruncateCore(int length)
    {
        if (length < _sent)
            throw new InvalidOperationException("The content to rewind has already been sent.");

        _window.Length = length - _sent;
        return _window.Length > 0 ? _window[_window.Length - 1] : '\n';
    }
    protected override void OnLineWritten()
    {
        if (_window.Length > SendThreshold)
            Send(_window.Length - WindowLength);
    }

    private void Send(int count)
    {
        if (count <= 0)
            return;

        _writer.Write(_window.ToString(0, count));
        _window.Remove(0, count);
        _sent += count;
    }
}

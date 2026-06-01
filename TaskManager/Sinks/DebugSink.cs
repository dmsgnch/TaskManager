using System.Diagnostics;
using System.Globalization;
using System.Text;
using Serilog.Core;
using Serilog.Events;

namespace TaskManager.Sinks;

public class DebugSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;

    public DebugSink(IFormatProvider? formatProvider = null)
    {
        this._formatProvider = formatProvider;
    }

    public void Emit(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;

        try
        {
            var message = BuildMessage(logEvent);

            WriteDebug(message);
        }
        catch (Exception ex)
        {
            WriteDebug($"[DebugSink failure] {ex}");
        }
    }

    private string BuildMessage(LogEvent logEvent)
    {
        var sb = new StringBuilder();

        sb.Append('[')
            .Append(logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture))
            .Append("] [")
            .Append(logEvent.Level)
            .Append("] ");

        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            sb.Append('[').Append(sourceContext).Append("] ");
        }

        sb.Append(logEvent.RenderMessage(_formatProvider));

        if (logEvent.Properties.Count > 0)
        {
            var extraProperties = logEvent.Properties
                .Where(p => p.Key != "SourceContext")
                .Select(p => $"{p.Key}={p.Value}");

            var propsText = string.Join(", ", extraProperties);

            if (!string.IsNullOrWhiteSpace(propsText))
            {
                sb.AppendLine();
                sb.Append("Properties: ").Append(propsText);
            }
        }

        if (logEvent.Exception is not null)
        {
            sb.AppendLine();
            sb.Append("Exception: ").Append(logEvent.Exception);
        }

        return sb.ToString();
    }

    [Conditional("DEBUG")]
    private static void WriteDebug(string message)
    {
        Debug.WriteLine(message);
    }
}
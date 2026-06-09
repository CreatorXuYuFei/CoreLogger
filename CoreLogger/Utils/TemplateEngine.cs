using CoreLogger.Entiy;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreLogger.Utils
{
    /// <summary>
    /// 模板引擎
    /// </summary>
    internal static partial class TemplateEngine
    {
        public static string Render(string template, LogMessage msg)
        {
            var sb = new StringBuilder(template);

            sb.Replace("{Module}", msg.Module)
              .Replace("{Level}", msg.Level.ToString())
              .Replace("{ThreadId}", msg.ThreadId.ToString())
              .Replace("{Message}", msg.Message)
              .Replace("{Exception}", GetExMsg(msg.Exception));

            var tsMatch = TsMatchRegex().Match(template);
            if (tsMatch.Success)
                sb.Replace(tsMatch.Value, msg.Time.ToString(tsMatch.Groups[1].Value));

            var scopeStr = string.Join(" ", msg.Scope.Select(kv => $"{kv.Key}={kv.Value}"));
            sb.Replace("{Scope}", scopeStr);

            var scopePropMatches = scopeRegex().Matches(template);
            foreach (Match match in scopePropMatches.Cast<Match>())
            {
                var key = match.Groups[1].Value;
                var val = msg.Scope.TryGetValue(key, out var v) ? v.ToString() : string.Empty;
                sb.Replace(match.Value, val);
            }

            return sb.ToString();
        }

        private static string GetExMsg(Exception? ex)
        {
            return ex == null ? string.Empty : $"[{ex.Message} | {ex.StackTrace}]";
        }

        [GeneratedRegex(@"{Scope\.(\w+)}")]
        private static partial Regex scopeRegex();
        [GeneratedRegex(@"{UtcTimestamp:(.*?)}")]
        private static partial Regex TsMatchRegex();
    }
}

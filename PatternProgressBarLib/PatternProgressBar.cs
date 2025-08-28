using Spectre.Console.PaternProgress;
using Spectre.Console.Rendering;
using System.Globalization;
using Wcwidth;

namespace Spectre.Console.PatternProgress;

internal sealed class PatternProgressBar : Renderable, IHasCulture
{
    private static bool ContainsUnicode(string s)
    {
        foreach (var c in s)
        {
            if (c > 127)
                return true;
        }
        return false;
    }
    public ProgressPattern ProgressPattern { get; set; } = ProgressPattern.Known.Default;
    public double Value { get; set; }
    public double MaxValue { get; set; } = 100;
    public int Width { get; set; } = 40;
    public Style FilledStyle { get; set; } = new(foreground: Color.Green);
    public Style FillingStyle { get; set; } = new(foreground: Color.Orange1);
    public Style EmptyStyle { get; set; } = new(foreground: Color.Grey35);
    public string Prefix { get; set; } = "[";
    public string Suffix { get; set; } = "]";
    public bool IsIndeterminate { get; set; }
    public CultureInfo? Culture { get; set; }

    private int _phase;

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(Width, maxWidth);
        return new Measurement(4, width + Prefix.Length + Suffix.Length);
    }


    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var useAscii = !options.Unicode && ProgressPattern.IsUnicode;
        var progressPattern = useAscii ? ProgressPattern.Known.Ascii : ProgressPattern ?? ProgressPattern.Known.Default;

        // If prefix or suffix contain Unicode, use ASCII [ and ] instead
        string prefix = useAscii && ContainsUnicode(Prefix) ? "[" : Prefix;
        string suffix = useAscii && ContainsUnicode(Suffix) ? "]" : Suffix;

        var pattern = progressPattern.Pattern;
        int barWidth = Math.Min(Width, maxWidth - prefix.Length - suffix.Length);
        if (barWidth <= 0)
            yield break;

        if (IsIndeterminate)
        {
            foreach (var segment in RenderIndeterminate(options, barWidth, pattern, prefix, suffix))
                yield return segment;
            yield break;
        }

        double progress = Math.Clamp(Value / MaxValue, 0, 1);
        int totalColumns = barWidth;
        int filledColumns = (int)Math.Floor(progress * totalColumns);
        double remainder = (progress * totalColumns) - filledColumns;
        int partialIndex = (int)Math.Floor(remainder * (pattern.Count - 1));

        yield return new Segment(prefix, Style.Plain);

        // Render bar using Unicode-aware width calculation
        int columnsRendered = 0;
        // Full cells
        while (columnsRendered < filledColumns)
        {
            var ch = pattern[^1];
            int w = UnicodeCalculator.GetWidth(ch);
            if (columnsRendered + w > filledColumns)
                break;
            yield return new Segment(ch.ToString(), FilledStyle);
            columnsRendered += w;
        }

        // Partial cell (only if not at the end)
        if (columnsRendered < totalColumns)
        {
            if (partialIndex > 0)
            {
                var ch = pattern[partialIndex];
                int w = UnicodeCalculator.GetWidth(ch);
                if (columnsRendered + w <= totalColumns)
                {
                    yield return new Segment(ch.ToString(), FillingStyle);
                    columnsRendered += w;
                }
            }
            else
            {
                var ch = pattern[0];
                int w = UnicodeCalculator.GetWidth(ch);
                if (columnsRendered + w <= totalColumns)
                {
                    yield return new Segment(ch.ToString(), EmptyStyle);
                    columnsRendered += w;
                }
            }
        }

        // Remaining empty cells
        while (columnsRendered < totalColumns)
        {
            var ch = pattern[0];
            int w = UnicodeCalculator.GetWidth(ch);
            if (columnsRendered + w > totalColumns)
                break;
            yield return new Segment(ch.ToString(), EmptyStyle);
            columnsRendered += w;
        }

        yield return new Segment(suffix, Style.Plain);
    }

    private IEnumerable<Segment> RenderIndeterminate(RenderOptions options, int width, IReadOnlyList<char> pattern, string prefix, string suffix)
    {
        _phase = (_phase + 1) % (width * (pattern.Count - 1));
        int activeCell = _phase / (pattern.Count - 1);
        int density = _phase % (pattern.Count - 1);
        yield return new Segment(prefix, Style.Plain);
        for (int i = 0; i < width; i++)
        {
            if (i == activeCell)
                yield return new Segment(pattern[density + 1].ToString(), FilledStyle);
            else
                yield return new Segment(pattern[0].ToString(), EmptyStyle);
        }
        yield return new Segment(suffix, Style.Plain);
    }
}

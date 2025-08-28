using Spectre.Console.PaternProgress;
using Spectre.Console.Rendering;
using System.Globalization;

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
        var width = Math.Min(Width, maxWidth - prefix.Length - suffix.Length);
        if (IsIndeterminate)
        {
            foreach (var segment in RenderIndeterminate(options, width, pattern, prefix, suffix))
                yield return segment;
            yield break;
        }

        double progress = Math.Clamp(Value / MaxValue, 0, 1);
        double totalUnits = width;
        double scaled = progress * totalUnits;
        int fullUnits = (int)Math.Floor(scaled);
        double remainder = scaled - fullUnits;
        int partialIndex = (int)Math.Floor(remainder * (pattern.Count - 1));

        yield return new Segment(prefix, Style.Plain);

        // Full cells
        for (int i = 0; i < fullUnits && i < width; i++)
            yield return new Segment(pattern[^1].ToString(), FilledStyle);

        // Partial cell (only if not at the end)
        if (fullUnits < width)
        {
            if (partialIndex > 0)
                yield return new Segment(pattern[partialIndex].ToString(), FillingStyle);
            else
                yield return new Segment(pattern[0].ToString(), EmptyStyle);
        }

        // Remaining empty cells
        int consumed = fullUnits + 1;
        for (int i = consumed; i < width; i++)
            yield return new Segment(pattern[0].ToString(), EmptyStyle);

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

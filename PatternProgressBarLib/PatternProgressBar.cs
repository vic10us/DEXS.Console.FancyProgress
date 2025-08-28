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
    public Style IndeterminateStyle { get; set; } = DefaultPulseStyle;
    public char UnicodeBar { get; set; } = '━';
    public char AsciiBar { get; set; } = '-';
    private const int PULSESIZE = 20;
    private const int PULSESPEED = 15;

    internal static Style DefaultPulseStyle { get; } = new Style(foreground: Color.DodgerBlue1, background: Color.Grey23);

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
            foreach (var segment in RenderIndeterminate(options, barWidth, prefix, suffix))
                yield return segment;
            yield break;
        }

        // Use IsCursor property for cursor mode
        if (ProgressPattern != null && ProgressPattern.IsCursor)
        {
            // Render a bar of background (pattern[0]), with a single cursor (pattern[1]) at the progress position
            yield return new Segment(prefix, Style.Plain);
            int cursorColumns = barWidth;
            int cursorPos = (int)Math.Round(Math.Clamp(Value / MaxValue, 0, 1) * (cursorColumns - 1));
            for (int i = 0; i < cursorColumns; i++)
            {
                if (i == cursorPos)
                    yield return new Segment(pattern[1].ToString(), FilledStyle);
                else
                    yield return new Segment(pattern[0].ToString(), EmptyStyle);
            }
            yield return new Segment(suffix, Style.Plain);
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
            int w = ch.GetWidth();
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
                int w = ch.GetWidth();
                if (columnsRendered + w <= totalColumns)
                {
                    yield return new Segment(ch.ToString(), FillingStyle);
                    columnsRendered += w;
                }
            }
            else
            {
                var ch = pattern[0];
                int w = ch.GetWidth();
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
            int w = ch.GetWidth();
            if (columnsRendered + w > totalColumns)
                break;
            yield return new Segment(ch.ToString(), EmptyStyle);
            columnsRendered += w;
        }


        yield return new Segment(suffix, Style.Plain);
    }

    private IEnumerable<Segment> RenderIndeterminate(RenderOptions options, int width, string prefix, string suffix)
    {
        var bar = options.Unicode ? UnicodeBar.ToString() : AsciiBar.ToString();
        var style = IndeterminateStyle ?? DefaultPulseStyle;

        IEnumerable<Segment> GetPulseSegments()
        {
            // For 1-bit and 3-bit colors, fall back to
            // a simpler versions with only two colors.
            if (options.ColorSystem is ColorSystem.NoColors or ColorSystem.Legacy)
            {
                // First half of the pulse
                var segments = Enumerable.Repeat(new Segment(bar, new Style(style.Foreground)), PULSESIZE / 2);

                // Second half of the pulse
                var legacy = options.ColorSystem is ColorSystem.NoColors or ColorSystem.Legacy;
                var bar2 = legacy ? " " : bar;
                segments = segments.Concat(Enumerable.Repeat(new Segment(bar2, new Style(style.Background)), PULSESIZE - (PULSESIZE / 2)));

                foreach (var segment in segments)
                {
                    yield return segment;
                }

                yield break;
            }

            for (var index = 0; index < PULSESIZE; index++)
            {
                var position = index / (float)PULSESIZE;
                var fade = 0.5f + ((float)Math.Cos(position * Math.PI * 2) / 2.0f);
                var color = style.Foreground.Blend(style.Background, fade);

                yield return new Segment(bar, new Style(foreground: color));
            }
        }

        // Get the pulse segments
        var pulseSegments = GetPulseSegments();
        pulseSegments = pulseSegments.Repeat((width / PULSESIZE) + 2);

        // Repeat the pulse segments
        var currentTime = (DateTime.Now - DateTime.Today).TotalSeconds;
        var offset = (int)(currentTime * PULSESPEED) % PULSESIZE;

        // Yield prefix
        yield return new Segment(prefix, Style.Plain);
        // Yield the bar
        foreach (var seg in pulseSegments.Skip(offset).Take(width))
            yield return seg;
        // Yield suffix
        yield return new Segment(suffix, Style.Plain);
    }
}

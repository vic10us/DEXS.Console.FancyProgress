using Spectre.Console;
using Spectre.Console.Rendering;
using System.Globalization;

namespace DEXS.Console.PatternProgress;

internal sealed class PatternProgressBar : Renderable, IHasCulture
{
    public ProgressPattern ProgressPattern { get; set; } = ProgressPattern.Known.Default;
    public Style IndeterminateStyle { get; set; } = DefaultPulseStyle;
    public string UnicodeBar { get; set; } = "━";
    public string AsciiBar { get; set; } = "-";
    public double Value { get; set; }
    public double MaxValue { get; set; } = 100;
    public int Width { get; set; } = 40;
    
    /// <summary>
    /// Style for the filled (completed) part of the progress bar.
    /// </summary>
    /// <returns></returns>
    public Style CompletedStyle { get; set; } = new(foreground: Color.Green);

    /// <summary>
    /// Style for the filling (in-progress) part of the progress bar.
    /// </summary>
    public Style ProgressStyle { get; set; } = new(foreground: Color.Orange1);

    /// <summary>
    /// Style for the filling (in-progress) part of the progress bar.
    /// Used for gradient if the foreground/background are not Color.Default
    /// </summary>
    public Style ProgressEndStyle { get; set; } = new(foreground: Color.Default);

    /// <summary>
    /// Style for the remaining (unfilled) part of the progress bar.
    /// </summary>
    public Style RemainingStyle { get; set; } = new(foreground: Color.Grey35);

    public string Prefix { get; set; } = "";
    public string Suffix { get; set; } = "";
    public bool IsIndeterminate { get; set; }
    public CultureInfo? Culture { get; set; }

    private const int PULSESIZE = 20;
    private const int PULSESPEED = 15;
    internal static Style DefaultPulseStyle { get; } = new Style(foreground: Color.DodgerBlue1, background: Color.Grey23);

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(Width, maxWidth);
        return new Measurement(4, width + Prefix.Length + Suffix.Length);
    }
    
    internal static Color BlendColor(Color start, Color end, float t)
    {
        if (start == end) return end;
        var blend = start.Blend(end, t);
        return (blend == 0) ? end : blend;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var useAscii = !options.Unicode && ProgressPattern.IsUnicode;
        var progressPattern = useAscii ? ProgressPattern.Known.AsciiBar : ProgressPattern ?? ProgressPattern.Known.Default;

        // If prefix or suffix contain Unicode, use ASCII [ and ] instead
        string prefix = useAscii && Prefix.ContainsUnicode() ? "" : Prefix;
        string suffix = useAscii && Suffix.ContainsUnicode() ? "" : Suffix;

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
            int cursorPos = (int)Math.Round(ClampCompat(Value / MaxValue, 0, 1) * (cursorColumns - 1));
            for (int i = 0; i < cursorColumns; i++)
            {
                if (i == cursorPos)
                    yield return new Segment(pattern[1], CompletedStyle);
                else
                    yield return new Segment(pattern[0], RemainingStyle);
            }
            yield return new Segment(suffix, Style.Plain);
            yield break;
        }

        double progress = ClampCompat(Value / MaxValue, 0, 1);
        int filledColumns = (int)Math.Floor(progress * barWidth);
        var completedBarCount = Math.Min(MaxValue, Math.Max(0, Value));
        var isCompleted = completedBarCount >= MaxValue;
        double remainder = (progress * barWidth) - filledColumns;
        int partialIndex = (int)Math.Floor(remainder * (pattern.Count - 1));
        var style = isCompleted ? CompletedStyle : ProgressStyle;

        yield return new Segment(prefix, style);

        int columnsRendered = 0;

        // Full cells with optional gradient
        for (int i = 0; columnsRendered < filledColumns;)
        {
            var ch = pattern[pattern.Count - 1];
            int w = ch.GetWidth();
            
            if (columnsRendered + w > filledColumns)
                break;

            Style cellStyle = style;
            
            if (!isCompleted && (ProgressEndStyle.Foreground != Color.Default || ProgressEndStyle.Background != Color.Default))
            {
                // Calculate gradient color for this position
                double t = (double)i / (filledColumns - 1);
                
                // Compute the foreground and background gradient based on the ProgressEndStyle
                var fgColor = BlendColor(ProgressEndStyle.Foreground, style.Foreground, (float)t);
                var bgColor = BlendColor(ProgressEndStyle.Background, style.Background, (float)t);

                cellStyle = new Style(foreground: fgColor, background: bgColor);
            }
            yield return new Segment(ch, cellStyle);
            columnsRendered += w;
            i++;
        }

        // Partial cell (only if not at the end)
        if (columnsRendered < barWidth)
        {
            if (partialIndex > 0)
            {
                var ch = pattern[partialIndex];
                int w = ch.GetWidth();
                if (columnsRendered + w <= barWidth)
                {
                    yield return new Segment(ch, ProgressStyle);
                    columnsRendered += w;
                }
            }
            else
            {
                var ch = pattern[0];
                int w = ch.GetWidth();
                if (columnsRendered + w <= barWidth)
                {
                    yield return new Segment(ch, RemainingStyle);
                    columnsRendered += w;
                }
            }
        }

        // Remaining empty cells
        while (columnsRendered < barWidth)
        {
            var ch = pattern[0];
            int w = ch.GetWidth();
            if (columnsRendered + w > barWidth)
                break;
            yield return new Segment(ch, RemainingStyle);
            columnsRendered += w;
        }

        yield return new Segment(suffix, style);
    }

    private IEnumerable<Segment> RenderIndeterminate(RenderOptions options, int width, string prefix, string suffix)
    {
        var bar = options.Unicode ? UnicodeBar : AsciiBar;
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
    // Polyfill for Math.Clamp for netstandard2.0
#if NETSTANDARD2_0
    private static double ClampCompat(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
#else
    private static double ClampCompat(double value, double min, double max)
        => Math.Clamp(value, min, max);
#endif
}

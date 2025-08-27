using Spectre.Console;
using Spectre.Console.Rendering;
using System.Globalization;

namespace progress_bar_dotnet;

internal sealed class BrailleProgressBar : Renderable, IHasCulture
{
    private static readonly char[] Braille = [
        ' ',
        '⡀',
        '⣀',
        '⣄',
        '⣤',
        '⣦',
        '⣶',
        '⣷',
        '⣿'
    ];

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

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(Width, maxWidth);
        return new Measurement(4, width + Prefix.Length + Suffix.Length);
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(Width, maxWidth - Prefix.Length - Suffix.Length);
        if (IsIndeterminate)
        {
            foreach (var segment in RenderIndeterminate(options, width))
                yield return segment;
            yield break;
        }

        double progress = Math.Clamp(Value / MaxValue, 0, 1);
        double totalUnits = width;
        double scaled = progress * totalUnits;
        int fullUnits = (int)Math.Floor(scaled);
        double remainder = scaled - fullUnits;
        int partialIndex = (int)Math.Floor(remainder * (Braille.Length - 1));

        yield return new Segment(Prefix, Style.Plain);

        // Full cells
        for (int i = 0; i < fullUnits && i < width; i++)
            yield return new Segment(Braille[^1].ToString(), FilledStyle);

        // Partial cell (only if not at the end)
        if (fullUnits < width)
        {
            if (partialIndex > 0)
                yield return new Segment(Braille[partialIndex].ToString(), FillingStyle);
            else
                yield return new Segment(Braille[0].ToString(), EmptyStyle);
        }

        // Remaining empty cells
        int consumed = fullUnits + 1;
        for (int i = consumed; i < width; i++)
            yield return new Segment(Braille[0].ToString(), EmptyStyle);

        yield return new Segment(Suffix, Style.Plain);
    }

    private int _phase;
    private IEnumerable<Segment> RenderIndeterminate(RenderOptions options, int width)
    {
        _phase = (_phase + 1) % (width * (Braille.Length - 1));
        int activeCell = _phase / (Braille.Length - 1);
        int density = _phase % (Braille.Length - 1);
        yield return new Segment(Prefix, Style.Plain);
        for (int i = 0; i < width; i++)
        {
            if (i == activeCell)
                yield return new Segment(Braille[density + 1].ToString(), FilledStyle);
            else
                yield return new Segment(Braille[0].ToString(), EmptyStyle);
        }
        yield return new Segment(Suffix, Style.Plain);
    }
}

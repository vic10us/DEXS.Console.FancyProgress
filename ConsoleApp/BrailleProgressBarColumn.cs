using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace progress_bar_dotnet;

public sealed class BrailleProgressBarColumn : ProgressColumn
{
    // Braille/dot characters (increasing density)
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

    public int Width { get; set; } = 40;
    public Style FilledStyle { get; set; } = new(foreground: Color.Green);
    public Style FillingStyle { get; set; } = new(foreground: Color.Orange1);
    public Style EmptyStyle { get; set; } = new(foreground: Color.Grey35);

    public string Prefix { get; set; } = "[[";
    public string Suffix { get; set; } = "]]";

    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        if (task.IsIndeterminate)
            return new Markup($"{Prefix}[dim]{GetIndeterminatePattern()}[/]{Suffix}");

        double progress = task.Percentage / 100.0;
        progress = Math.Clamp(progress, 0, 1);
        double totalUnits = Width;
        double scaled = progress * totalUnits;
        int fullUnits = (int)Math.Floor(scaled);
        double remainder = scaled - fullUnits;

        // Pick braille partial index
        int partialIndex = (int)Math.Floor(remainder * (Braille.Length - 1));

        var sb = new StringBuilder();
        sb.Append(Prefix);

        // Full cells
        for (int i = 0; i < fullUnits && i < Width; i++)
            sb.Append(Braille[^1].ToString().ApplyStyle(FilledStyle));

        // Partial cell (only if not at the end)
        if (fullUnits < Width)
        {
            if (partialIndex > 0)
                sb.Append(Braille[partialIndex].ToString().ApplyStyle(FillingStyle));
            else
                sb.Append(Braille[0].ToString().ApplyStyle(EmptyStyle));
        }

        // Remaining empty cells
        int consumed = fullUnits + 1;
        for (int i = consumed; i < Width; i++)
            sb.Append(Braille[0].ToString().ApplyStyle(EmptyStyle));

        sb.Append(Suffix);
        return new Markup(sb.ToString());
    }

    private int _phase;
    private string GetIndeterminatePattern()
    {
        _phase = (_phase + 1) % (Width * (Braille.Length - 1));
        int activeCell = _phase / (Braille.Length - 1);
        int density = _phase % (Braille.Length - 1);
        var sb = new StringBuilder(Width);
        for (int i = 0; i < Width; i++)
        {
            if (i == activeCell)
                sb.Append($"[bold {FilledStyle.Foreground.ToString() ?? "default"}]{Braille[density + 1]}[/]");
            else
                sb.Append($"[dim]{Braille[0]}[/]");
        }

        return sb.ToString();
    }
}

file static class MarkupExtensions
{
    public static string ApplyStyle(this string escaped, Style style)
    {
        var fg = style.Foreground.ToString();
        var bg = style.Background.ToString();
        var deco = style.Decoration != Decoration.None
            ? style.Decoration.ToMarkup()
            : null;

        var open = new StringBuilder();
        if (fg != null) open.Append(fg);
        if (bg != null) open.Append($" on {bg}");
        if (deco != null)
        {
            if (open.Length > 0) open.Append(' ');
            open.Append($"[{deco}]");
        }

        return open.Length > 0
            ? $"[{open}]{escaped}[/]"
            : escaped;
    }

    private static string ToMarkup(this Decoration decoration)
    {
        return decoration switch
        {
            Decoration.Bold => "bold",
            Decoration.Dim => "dim",
            Decoration.Italic => "italic",
            Decoration.Underline => "underline",
            Decoration.SlowBlink => "slowblink",
            Decoration.RapidBlink => "rapidblink",
            Decoration.Invert => "inverse",
            Decoration.Conceal => "conceal",
            Decoration.Strikethrough => "strikethrough",
            _ => ""
        };
    }
}


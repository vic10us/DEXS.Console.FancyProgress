using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace progress_bar_dotnet;

public sealed class BrailleProgressBarColumn : ProgressColumn
{
    public int Width { get; set; } = 40;
    public Style FilledStyle { get; set; } = new(foreground: Color.Green);
    public Style FillingStyle { get; set; } = new(foreground: Color.Orange1);
    public Style EmptyStyle { get; set; } = new(foreground: Color.Grey35);

    public string Prefix { get; set; } = "[";
    public string Suffix { get; set; } = "]";

    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        return new BrailleProgressBar
        {
            Value = task.Value,
            MaxValue = task.MaxValue,
            Width = this.Width,
            FilledStyle = this.FilledStyle,
            FillingStyle = this.FillingStyle,
            EmptyStyle = this.EmptyStyle,
            Prefix = this.Prefix,
            Suffix = this.Suffix,
            IsIndeterminate = task.IsIndeterminate
        };
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


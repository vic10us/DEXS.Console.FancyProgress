using Spectre.Console;
using Spectre.Console.Rendering;

namespace progress_bar_dotnet;

public sealed class PatternProgressBarColumn : ProgressColumn
{
    public int Width { get; set; } = 40;
    public Style FilledStyle { get; set; } = new(foreground: Color.Green);
    public Style FillingStyle { get; set; } = new(foreground: Color.Orange1);
    public Style EmptyStyle { get; set; } = new(foreground: Color.Grey35);
    public string Prefix { get; set; } = "[";
    public string Suffix { get; set; } = "]";
    public char[] Pattern { get; set; } = PatternProgressBar.Braille;

    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        return new PatternProgressBar
        {
            Value = task.Value,
            MaxValue = task.MaxValue,
            Width = this.Width,
            FilledStyle = this.FilledStyle,
            FillingStyle = this.FillingStyle,
            EmptyStyle = this.EmptyStyle,
            Prefix = this.Prefix,
            Suffix = this.Suffix,
            IsIndeterminate = task.IsIndeterminate,
            Pattern = this.Pattern
        };
    }
}

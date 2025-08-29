using DEXS.Console.PatternProgress;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DEXS.Console.PatternProgress;

public sealed class PatternProgressBarColumn : ProgressColumn
{
    public int Width { get; set; } = 40;
    public Style CompletedStyle { get; set; } = new(foreground: Color.Green);
    public Style PartiallyCompletedStyle { get; set; } = new(foreground: Color.Orange1);
    public Style RemainingStyle { get; set; } = new(foreground: Color.Grey35);
    public string Prefix { get; set; } = "[";
    public string Suffix { get; set; } = "]";
    public ProgressPattern ProgressPattern { get; set; } = ProgressPattern.Known.Default;

    /// <summary>
    /// Gets or sets the style of an indeterminate progress bar.
    /// </summary>
    public Style IndeterminateStyle { get; set; } = PatternProgressBar.DefaultPulseStyle;

    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        return new PatternProgressBar
        {
            Value = task.Value,
            MaxValue = task.MaxValue,
            Width = Width,
            CompletedStyle = CompletedStyle,
            PartiallyCompletedStyle = PartiallyCompletedStyle,
            RemainingStyle = RemainingStyle,
            Prefix = Prefix,
            Suffix = Suffix,
            IsIndeterminate = task.IsIndeterminate,
            IndeterminateStyle = IndeterminateStyle,
            ProgressPattern = ProgressPattern
        };
    }
}

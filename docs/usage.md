# DEXS.Console.PatternProgress Usage Guide

## Overview

`DEXS.Console.PatternProgress` provides a drop-in, highly customizable progress bar column for Spectre.Console, supporting Unicode, emoji, and custom patterns.

---

## Basic Example

```csharp
using Spectre.Console;
using DEXS.Console.PatternProgress;

AnsiConsole.Progress()
    .AutoClear(false)
    .Columns(
        new TaskDescriptionColumn(),
        new PatternProgressBarColumn
        {
            Width = 40,
            FilledStyle = new Style(foreground: Color.Green),
            FillingStyle = new Style(foreground: Color.Yellow4),
            EmptyStyle = new Style(foreground: Color.Grey35),
            ProgressPattern = ProgressPattern.Known.Braille,
        },
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn { Spinner = Spinner.Known.Dots12 }
    )
    .Start(ctx =>
    {
        var task = ctx.AddTask("[green1]Processing...[/]", maxValue: 100);
        while (!ctx.IsFinished)
        {
            task.Increment(1.5);
            Thread.Sleep(20);
        }
    });
```

---

## Using Custom Patterns at Runtime

You can define your own progress bar patterns at runtime by subclassing `ProgressPattern` directly in your application code—no need to edit the JSON or recompile the library. Example:

```csharp
using DEXS.Console.PatternProgress;

public class MyCustomPattern : ProgressPattern
{
    public override string Name => "My Custom";
    public override bool IsUnicode => true;
    public override bool IsCursor => false;
    public override IReadOnlyList<string> Pattern => new[] { 
        "░", 
        "█"
    };
}

// Usage:
var customPattern = new MyCustomPattern();

AnsiConsole.Progress()
    .Columns(
        new PatternProgressBarColumn { 
            ProgressPattern = customPattern 
        }
        // ... other columns ...
    )
    .Start(ctx => { /* ... */ });
```

```csharp
foreach (var pattern in ProgressPattern.Known.AllPatterns)
{
    AnsiConsole.WriteLine($"Pattern: {pattern.Name} - {string.Join("", pattern.Pattern)}");
}
```

---

## Customizing Patterns

- To add your own patterns, edit `progressPatterns.json` in the generator project and rebuild.
- Each pattern can use Unicode, emoji, or ASCII.

Example JSON entry:

```json
{
  "stars": {
    "pattern": ["☆", "★"],
    "isDefault": false,
    "isCursor": false
  }
}
```

---


## Gradient Support

You can enable a color gradient for the filled portion of the progress bar by setting both `ProgressStyle` and `ProgressEndStyle` on the `PatternProgressBarColumn`. The bar will smoothly blend from the foreground color of `ProgressStyle` (start) to the foreground color of `ProgressEndStyle` (end) as progress increases.

**How to enable a gradient:**

```csharp
AnsiConsole.Progress()
    .Columns(
        new PatternProgressBarColumn
        {
            Width = 40,
            ProgressStyle = new Style(foreground: new Color(0, 255, 163)), // Start color
            ProgressEndStyle = new Style(foreground: new Color(177, 79, 255)), // End color
            RemainingStyle = new Style(foreground: Color.Grey35),
            ProgressPattern = ProgressPattern.Known.UnicodeBar
        },
        // ... other columns ...
    )
    .Start(ctx => { /* ... */ });
```

**Properties:**

- `ProgressStyle`: The style (color) at the start of the filled bar (left side).
- `ProgressEndStyle`: The style (color) at the end of the filled bar (right side, at current progress). Set the foreground to a color to enable the gradient.
- `RemainingStyle`: The style for the unfilled portion.

If `ProgressEndStyle.Foreground` is set to `Color.Default`, no gradient is applied and the bar uses a solid color from `ProgressStyle`.

---

## Advanced: Indeterminate Bars & Cursor Mode

- Use `IsIndeterminate` on the task for animated bars.
- Use patterns with `isCursor: true` for cursor-style progress.

---

## API Reference

- `PatternProgressBarColumn` (main column type)
- `ProgressPattern.Known` (all built-in patterns)
- `ProgressPattern` (base class for custom patterns)

---

For more, see the root [README.md](../README.md).

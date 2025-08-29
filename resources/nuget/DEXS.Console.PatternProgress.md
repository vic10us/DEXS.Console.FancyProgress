# DEXS.Console.PatternProgress Usage

## Overview

`DEXS.Console.PatternProgress` provides a drop-in, highly customizable progress bar column for Spectre.Console, supporting Unicode, emoji, and custom patterns.

![resources/gfx/demo.gif](https://raw.githubusercontent.com/vic10us/Spectre.Console.PatternProgress/refs/heads/main/resources/gfx/demo.gif)

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

## Advanced: Indeterminate Bars & Cursor Mode

- Use `IsIndeterminate` on the task for animated bars.
- Use patterns with `IsCursor: true` for cursor-style progress.

---

## API Reference

- `PatternProgressBarColumn` (main column type)
- `ProgressPattern.Known` (all built-in patterns)
- `ProgressPattern` (base class for custom patterns)


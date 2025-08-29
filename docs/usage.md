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

## Display All Built-in Patterns

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

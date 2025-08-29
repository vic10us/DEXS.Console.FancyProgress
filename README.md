# DEXS.Console.PatternProgress

A multi-targeted, emoji-aware, and highly customizable progress bar column library for [Spectre.Console](https://spectreconsole.net/). Supports .NET 8, 9, 10, and netstandard2.0. No third-party dependencies.

![caption](resources/gfx/demo.gif)

## Features
- Unicode and emoji support (grapheme-aware)
- Custom progress bar patterns (including emoji, Unicode, ASCII)
- Source generator for easy pattern extension
- Multi-targeted: net8.0, net9.0, net10.0, netstandard2.0
- No third-party dependencies

## Quick Start

Install via NuGet:

```
dotnet add package DEXS.Console.PatternProgress
```

## Basic Usage

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

## All Built-in Patterns

```csharp
foreach (var pattern in ProgressPattern.Known.AllPatterns)
{
    AnsiConsole.WriteLine($"Pattern: {pattern.Name} - {string.Join("", pattern.Pattern)}");
}
```

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

---

## Custom Patterns (via Code Generator)

You can also add your own patterns by editing `progressPatterns.json` and rebuilding the project (requires recompiling the library).

## Documentation

See [docs/usage.md](docs/usage.md) for advanced usage, API details, and more examples.

---

MIT License | Copyright (c) 2025 vic10us

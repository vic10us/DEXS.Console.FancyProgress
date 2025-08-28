
using Spectre.Console;
using progress_bar_dotnet;

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
		// new ProgressBarColumn
		// {
		// 	IndeterminateStyle = new Style(foreground: Color.Green1)
		// },    // (Optional: Spectre's default bar)
		new PatternProgressBarColumn {
			Width = 30,
			FilledStyle = new Style(foreground: Color.Green),
			FillingStyle = new Style(foreground: Color.Green1),
			EmptyStyle = new Style(foreground: Color.Grey35),
			Pattern = PatternProgressBar.Braille
		},
		new RemainingTimeColumn(),      // Remaining time
		new SpinnerColumn()
		{
			Spinner = Spinner.Known.Dots12,
			Style = new Style(foreground: Color.Yellow)
		}
	)
	.Start(ctx =>
	{
		var task = ctx.AddTask("[green1]Processing...[/]", maxValue: 100);
		while (!ctx.IsFinished)
		{
			task.Increment(1.5);
			Thread.Sleep(150);
		}
	});


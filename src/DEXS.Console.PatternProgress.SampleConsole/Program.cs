using Spectre.Console;
using DEXS.Console.PatternProgress;

// Console.ReadLine();

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
        new ProgressBarColumn
        {
            Width = 40,
            
		},
		new PatternProgressBarColumn
		{
			Width = 40,
			CompletedStyle = new Style(foreground: Color.Green, background: null),
			PartiallyCompletedStyle = new Style(foreground: Color.Yellow4),
			RemainingStyle = new Style(foreground: Color.Grey35),
			ProgressPattern = ProgressPattern.Known.Braille,
		},
        new PercentageColumn(),
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

		task.IsIndeterminate = true;

        Thread.Sleep(1000);

		task.IsIndeterminate = false;

		while (!ctx.IsFinished)
		{
			task.Increment(1.5);
			Thread.Sleep(150);
		}
	});

using Spectre.Console;
using DEXS.Console.PatternProgress;

Console.ReadLine();

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
		new PatternProgressBarColumn
		{
			Width = 40,
			FilledStyle = new Style(foreground: Color.Green, background: null),
			FillingStyle = new Style(foreground: Color.Yellow4),
			EmptyStyle = new Style(foreground: Color.Grey35),
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

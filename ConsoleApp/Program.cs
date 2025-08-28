using Spectre.Console;
using Spectre.Console.PaternProgress;
using Spectre.Console.PatternProgress;

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
		new ProgressBarColumn
		{
			IndeterminateStyle = new Style(foreground: Color.Green1)
		},    // (Optional: Spectre's default bar)
		new PatternProgressBarColumn
		{
			Width = 40,
			FilledStyle = new Style(foreground: Color.Green),
			FillingStyle = new Style(foreground: Color.Yellow4),
			EmptyStyle = new Style(foreground: Color.Grey35),
			ProgressPattern = ProgressPattern.Known.Braille,
			Prefix = "｣",
			Suffix = " ｢"
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
        var task2 = ctx.AddTask("[green1]More Processing...[/]", maxValue: 100);
        var task3 = ctx.AddTask("[green1]Some Processing...[/]", maxValue: 100);
        
		task.IsIndeterminate = true;
        task2.IsIndeterminate = true;
        task3.IsIndeterminate = true;

        Thread.Sleep(2000);

		task.IsIndeterminate = false;

		while (!ctx.IsFinished)
		{
			task.Increment(1.5);
			Thread.Sleep(150);
		}
	});

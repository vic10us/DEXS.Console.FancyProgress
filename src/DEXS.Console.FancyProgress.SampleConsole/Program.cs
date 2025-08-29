using Spectre.Console;
using DEXS.Console.FancyProgress;

// Console.ReadLine();

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
		new FancyProgressBarColumn
		{
			Width = 40,
			CompletedStyle = new Style(foreground: Color.Green),
			CompletedTailStyle = new Style(foreground: new Color(0, 255, 163)),
			ProgressStyle = new Style(foreground: new Color(0, 255, 163)), //, background: Color.DodgerBlue1),
			ProgressTailStyle = new Style(foreground: new Color(177, 79, 255)), //, background: new Color(50, 0, 0)),
			RemainingStyle = new Style(foreground: Color.Grey35),
			ProgressPattern = ProgressPattern.Known.AsciiBar
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

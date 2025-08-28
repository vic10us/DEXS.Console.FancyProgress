using Spectre.Console;
using Spectre.Console.PatternProgress;

AnsiConsole.Progress()
	.AutoClear(false)
	.Columns(
		new TaskDescriptionColumn(),    // Task description
		//  new ProgressBarColumn
		//  {
		//  	IndeterminateStyle = new Style(foreground: Color.Green1)
		//  },    // (Optional: Spectre's default bar)
		new PatternProgressBarColumn
		{
			Width = 40,
			FilledStyle = new Style(foreground: Color.Green, background: null),
			FillingStyle = new Style(foreground: Color.Yellow4),
			EmptyStyle = new Style(foreground: Color.Grey35),
			ProgressPattern = ProgressPattern.Known.WhiteBlack,
			IndeterminateStyle = new Style(foreground: Color.Green1),
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
        //var task2 = ctx.AddTask("[green1]More Processing...[/]", maxValue: 100);
        //var task3 = ctx.AddTask("[green1]Some Processing...[/]", maxValue: 100);
        
		task.IsIndeterminate = true;
        //task2.IsIndeterminate = true;
        //task3.IsIndeterminate = true;

        Thread.Sleep(1000);

		task.IsIndeterminate = false;

		while (!ctx.IsFinished)
		{
			// if (task.Value >= task.MaxValue / 2)
			// {
			// 	task2.IsIndeterminate = false;
			// 	task2.Increment(1.5);
			// 	if (task2.Value >= task2.MaxValue / 2)
			// 	{
			// 		task3.IsIndeterminate = false;
			// 		task3.Increment(1.5);
			// 	}
			// }
			task.Increment(1.5);
			Thread.Sleep(20);
		}
	});

using Spectre.Console;
using DEXS.Console.FancyProgress;

// Cubic Bézier easing function
static double CubicBezier(double t, double p0, double p1, double p2, double p3)
{
	double u = 1 - t;
	return (u * u * u * p0)
		+ (3 * u * u * t * p1)
		+ (3 * u * t * t * p2)
		+ (t * t * t * p3);
}

var x = AnsiConsole.Progress();

x.RefreshRate = TimeSpan.FromMilliseconds(5);

x.AutoClear(false)
    .Columns(
		new TaskDescriptionColumn(),    // Task description
		new FancyProgressBarColumn
		{
			Width = 40,
			CompletedStyle = new Style(foreground: Color.SeaGreen1),
			CompletedTailStyle = new Style(foreground: Color.DarkGreen),
			ProgressStyle = new Style(foreground: Color.SeaGreen1),
			ProgressTailStyle = new Style(foreground: Color.SlateBlue1),
			RemainingStyle = new Style(foreground: Color.Grey35),
			ProgressPattern = ProgressPattern.Known.Braille
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

		int steps = 100;             // how many "frames" we expect
		int minDelay = 5;            // fastest delay
		int maxDelay = 100;          // slowest delay

		for (int i = 0; !ctx.IsFinished; i++)
		{
			task.Increment(0.5);

			double t = (double)i / steps;   // normalized progress (0 → 1)
			if (t > 1) t = 1;

			// Bézier ease-in (slow → fast)
			double eased = CubicBezier(t, 0, 0.42, 1, 1);

			// map eased value to delay
			int delay = (int)(maxDelay - eased * (maxDelay - minDelay));

			Thread.Sleep(delay);
		}
	});

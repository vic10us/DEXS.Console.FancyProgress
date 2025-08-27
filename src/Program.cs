using System;
using System.Threading;

class Program
{
	// Unicode block elements from full to smallest
	static readonly char[] Blocks = new char[] { ' ', '▏', '▎', '▍', '▌', '▋', '▊', '▉', '█' };
	// Dot/Braille style characters (increasing density)
	static readonly char[] Dots = new char[] { ' ', '⡀','⣀', '⣄', '⣤', '⣦', '⣶', '⣷', '⣿' };

	// ANSI color codes for foreground/background
	static readonly (string name, string fg, string bg)[] Colors = new[] {
		("Default", "", ""),
		("Red",     "\u001b[31m", "\u001b[41m"),
		("Green",   "\u001b[32m", "\u001b[42m"),
		("Yellow",  "\u001b[33m", "\u001b[43m"),
		("Blue",    "\u001b[34m", "\u001b[44m"),
		("Magenta", "\u001b[35m", "\u001b[45m"),
		("Cyan",    "\u001b[36m", "\u001b[46m"),
		("White",   "\u001b[37m", "\u001b[47m")
	};
	static readonly string Reset = "\u001b[0m";

	// Returns a progress bar string using Unicode blocks, with color (filled: fg+bg, empty: bg only)
	static string GetBlockProgressBar(double progress, int width, string fg, string bg)
	{
		progress = Math.Max(0, Math.Min(1, progress));
		int totalBlocks = width;
		double totalProgress = progress * totalBlocks;
		int fullBlocks = (int)Math.Floor(totalProgress);
		int partialBlockIndex = (int)Math.Round((totalProgress - fullBlocks) * (Blocks.Length - 1));

		var bar = new System.Text.StringBuilder();
		for (int i = 0; i < fullBlocks; i++)
			bar.Append(fg + bg + Blocks[Blocks.Length - 1] + Reset);
		if (fullBlocks < totalBlocks)
			bar.Append(fg + bg + Blocks[partialBlockIndex] + Reset);
		for (int i = fullBlocks + 1; i < totalBlocks; i++)
			bar.Append(bg + Blocks[0] + Reset);
		return bar.ToString();
	}

	// Returns a progress bar string using dot/braille style, with color (filled: fg+bg, empty: bg only)
	static string GetDotProgressBar(double progress, int width, string fg, string bg)
	{
		progress = Math.Max(0, Math.Min(1, progress));
		int totalDots = width;
		double totalProgress = progress * totalDots;
		int fullDots = (int)Math.Floor(totalProgress);
		int partialDotIndex = (int)Math.Round((totalProgress - fullDots) * (Dots.Length - 1));

		var bar = new System.Text.StringBuilder();
		for (int i = 0; i < fullDots; i++)
			bar.Append(fg + bg + Dots[Dots.Length - 1] + Reset);
		if (fullDots < totalDots)
			bar.Append(fg + bg + Dots[partialDotIndex] + Reset);
		for (int i = fullDots + 1; i < totalDots; i++)
			bar.Append(bg + Dots[0] + Reset);
		return bar.ToString();
	}

	static int SelectColor(string prompt)
	{
		Console.WriteLine(prompt);
		for (int i = 0; i < Colors.Length; i++)
		{
			var (name, fg, bg) = Colors[i];
			Console.WriteLine($"{i}. {fg}{bg}{name}{Reset}");
		}
		Console.Write("Enter number: ");
		if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 0 && idx < Colors.Length)
			return idx;
		return 0;
	}

	static void Main()
	{
		int barWidth = 40;
		Console.WriteLine("Choose progress bar style:");
		Console.WriteLine("1. Block (█ ▉ ▊ ...)");
		Console.WriteLine("2. Dot/Braille (⣀ ⣤ ⣶ ⣿ ...)");
		Console.Write("Enter 1 or 2: ");
		string? input = Console.ReadLine();
		bool useDot = input == "2";

		int fgIdx = SelectColor("Choose foreground (filled) color:");
		int bgIdx = SelectColor("Choose background (empty) color:");
		string fg = Colors[fgIdx].fg;
		string bg = Colors[bgIdx].bg;

		// Hide cursor
		Console.Write("\u001b[?25l");
		int lastLen = 0;
		for (int i = 0; i <= 100; i++)
		{
			double progress = i / 100.0;
			string bar = useDot ? GetDotProgressBar(progress, barWidth, fg, bg) : GetBlockProgressBar(progress, barWidth, fg, bg);
			string output = $"[{bar}] {i,3}%";
			// Pad with spaces to clear previous output
			int pad = Math.Max(0, lastLen - output.Length);
			Console.Write($"\r{output}{new string(' ', pad)}");
			lastLen = output.Length;
			Thread.Sleep(150);
		}
		// Show cursor
		Console.Write("\u001b[?25h");
		Console.WriteLine("\nDone!" + Reset);
	}
}

using System;
using System.Text;
using System.Threading;

namespace ProgressBarDotNet;
// This file has been renamed to ProgressBar.cs for clarity and best practices.

/// <summary>
/// High-resolution Unicode/ANSI progress bar for .NET console apps, inspired by Docker CLI.
/// </summary>
public class ProgressBar
{
	public enum Style { Block, Dot }

	// Unicode block elements from full to smallest
	static readonly char[] Blocks = new char[] { ' ', '▏', '▎', '▍', '▌', '▋', '▊', '▉', '█' };
	// Dot/Braille style characters (increasing density)
	static readonly char[] Dots = new char[] { ' ', '⡀','⣀', '⣄', '⣤', '⣦', '⣶', '⣷', '⣿' };

	/// <summary>
	/// Renders a progress bar string with the specified style and colors.
	/// </summary>
	/// <param name="progress">Progress value (0.0 to 1.0)</param>
	/// <param name="width">Bar width in characters</param>
	/// <param name="style">Progress bar style</param>
	/// <param name="fg">ANSI foreground color code (e.g., "\u001b[31m")</param>
	/// <param name="bg">ANSI background color code (e.g., "\u001b[47m")</param>
	/// <param name="reset">ANSI reset code (e.g., "\u001b[0m")</param>
	public static string Render(double progress, int width, Style style, string fg, string bg, string reset)
	{
		return style == Style.Dot
			? GetDotProgressBar(progress, width, fg, bg, reset)
			: GetBlockProgressBar(progress, width, fg, bg, reset);
	}

	private static string GetBlockProgressBar(double progress, int width, string fg, string bg, string reset)
	{
		progress = Math.Max(0, Math.Min(1, progress));
		int totalBlocks = width;
		double totalProgress = progress * totalBlocks;
		int fullBlocks = (int)Math.Floor(totalProgress);
		int partialBlockIndex = (int)Math.Round((totalProgress - fullBlocks) * (Blocks.Length - 1));

		var bar = new StringBuilder();
		for (int i = 0; i < fullBlocks; i++)
			bar.Append(fg + bg + Blocks[Blocks.Length - 1] + reset);
		if (fullBlocks < totalBlocks)
			bar.Append(fg + bg + Blocks[partialBlockIndex] + reset);
		for (int i = fullBlocks + 1; i < totalBlocks; i++)
			bar.Append(bg + Blocks[0] + reset);
		return bar.ToString();
	}

	private static string GetDotProgressBar(double progress, int width, string fg, string bg, string reset)
	{
		progress = Math.Max(0, Math.Min(1, progress));
		int totalDots = width;
		double totalProgress = progress * totalDots;
		int fullDots = (int)Math.Floor(totalProgress);
		int partialDotIndex = (int)Math.Round((totalProgress - fullDots) * (Dots.Length - 1));

		var bar = new StringBuilder();
		for (int i = 0; i < fullDots; i++)
			bar.Append(fg + bg + Dots[Dots.Length - 1] + reset);
		if (fullDots < totalDots)
			bar.Append(fg + bg + Dots[partialDotIndex] + reset);
		for (int i = fullDots + 1; i < totalDots; i++)
			bar.Append(bg + Dots[0] + reset);
		return bar.ToString();
	}
}

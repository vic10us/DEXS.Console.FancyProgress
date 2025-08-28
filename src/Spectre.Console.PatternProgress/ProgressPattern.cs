namespace Spectre.Console.PatternProgress;

public abstract partial class ProgressPattern
{
    /// <summary>
    /// Gets the name associated with the current instance.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the progress pattern uses Unicode characters.
    /// </summary>
    public abstract bool IsUnicode { get; }

    /// <summary>
    /// Gets a value indicating whether the progress pattern is a cursor pattern.
    /// </summary>
    public abstract bool IsCursor { get; }

    /// <summary>
    /// Gets the character pattern used for the progress bar.
    /// </summary>
    public abstract IReadOnlyList<char> Pattern { get; }
}

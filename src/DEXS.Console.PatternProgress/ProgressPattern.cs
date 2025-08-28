namespace DEXS.Console.PatternProgress;

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
    /// Gets the string pattern used for the progress bar (can be emoji or multi-char).
    /// </summary>
    public abstract IReadOnlyList<string> Pattern { get; }
}

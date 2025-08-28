namespace Spectre.Console.PaternProgress;

public abstract partial class ProgressPattern
{
    /// <summary>
    /// Gets a value indicating whether the progress pattern uses Unicode characters.
    /// </summary>
    /// <value></value>
    public abstract bool IsUnicode { get; }
    
    /// <summary>
    /// Gets the character pattern used for the progress bar.
    /// </summary>
    /// <value></value>
    public abstract IReadOnlyList<char> Pattern { get; }
}

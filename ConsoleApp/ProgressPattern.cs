namespace progress_bar_dotnet;

public abstract partial class ProgressPattern
{
    /// <summary>
    /// Gets the character pattern used for the progress bar.
    /// </summary>
    /// <value></value>
    public abstract IReadOnlyList<char> Pattern { get; }
}

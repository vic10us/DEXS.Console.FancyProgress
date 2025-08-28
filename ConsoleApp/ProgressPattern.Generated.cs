namespace progress_bar_dotnet;

public abstract partial class ProgressPattern
{
    private sealed class BrailleProgressPattern : ProgressPattern
    {
        public override IReadOnlyList<char> Pattern => [' ', '⡀', '⣀', '⣄', '⣤', '⣦', '⣶', '⣷', '⣿'];
    }

    private sealed class BlockProgressPattern : ProgressPattern
    {
        public override IReadOnlyList<char> Pattern => [' ', '▏', '▎', '▍', '▌', '▋', '▊', '▉', '█'];
    }

    public static class Known
    {
        public static ProgressPattern Default { get; } = new BrailleProgressPattern();
        public static ProgressPattern Braille { get; } = new BrailleProgressPattern();
        public static ProgressPattern Block { get; } = new BlockProgressPattern();
    }
}
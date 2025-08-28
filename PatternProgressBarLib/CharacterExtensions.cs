namespace Spectre.Console.PatternProgress;

internal static class CharacterExtensions
{
    public static int GetWidth(this char c)
    {
        // Control chars, non-spacing marks
        if (char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
            return 0;
        // Basic Latin, Latin-1 Supplement, etc.
        if (c < 0x1100)
            return 1;
        // Wide and fullwidth blocks (CJK, Hangul, Hiragana, Katakana, Fullwidth, etc.)
        if (
            (c >= 0x1100 && c <= 0x115F) || // Hangul Jamo
            (c >= 0x2329 && c <= 0x232A) ||
            (c >= 0x2E80 && c <= 0xA4CF && c != 0x303F) || // CJK ... Yi
            (c >= 0xAC00 && c <= 0xD7A3) || // Hangul Syllables
            (c >= 0xF900 && c <= 0xFAFF) || // CJK Compatibility Ideographs
            (c >= 0xFE10 && c <= 0xFE19) ||
            (c >= 0xFE30 && c <= 0xFE6F) ||
            (c >= 0xFF00 && c <= 0xFF60) || // Fullwidth Forms
            (c >= 0xFFE0 && c <= 0xFFE6) ||
            (c >= 0x2B00 && c <= 0x2BFF) // Misc Symbols and Arrows (⬜⬛)
        )
            return 2;
        // Emoji and Miscellaneous Symbols (U+1F300–U+1FAFF, U+20000–U+2FFFD, U+30000–U+3FFFD)
        // Note: char is 16-bit, so surrogate pairs are needed for codepoints > 0xFFFF
        // We'll treat high surrogates as width 2 for most emoji/symbols
        if (char.IsSurrogate(c))
            return 2;
        // Additional wide blocks (East Asian Wide/Fullwidth per Unicode 15.0)
        if (
            (c >= 0x2000 && c <= 0x206F) || // General Punctuation (some wide)
            (c >= 0x2100 && c <= 0x214F) || // Letterlike Symbols
            (c >= 0x2190 && c <= 0x21FF) || // Arrows
            (c >= 0x2200 && c <= 0x22FF) || // Math Operators
            (c >= 0x2300 && c <= 0x23FF) || // Misc Technical
            (c >= 0x2460 && c <= 0x24FF) || // Enclosed Alphanumerics
            (c >= 0x2500 && c <= 0x259F) || // Box Drawing, Block Elements
            (c >= 0x25A0 && c <= 0x25FF) || // Geometric Shapes
            (c >= 0x2600 && c <= 0x27BF) || // Misc Symbols, Dingbats
            (c >= 0x2FF0 && c <= 0x2FFF)
        )
            return 2;
        // Default to width 1
        return 1;
    }
}
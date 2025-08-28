namespace Spectre.Console.PatternProgress;

internal static class Extensions
{
    public static bool ContainsUnicode(this string s)
    {
        foreach (var c in s)
        {
            if (c > 127)
                return true;
        }
        return false;
    }

    public static int GetWidth(this char c)
    {
        // Control chars, non-spacing marks
        if (char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
            return 0;

        // Wide/Fullwidth per Unicode East Asian Width (W/F)
        // See: https://www.unicode.org/reports/tr11/ and EastAsianWidth.txt
        if (
            (c >= 0x1100 && c <= 0x115F) || // Hangul Jamo
            (c == 0x2329 || c == 0x232A) || // Angle brackets
            (c >= 0x2E80 && c <= 0xA4CF && c != 0x303F) || // CJK, Yi, etc.
            (c >= 0xAC00 && c <= 0xD7A3) || // Hangul Syllables
            (c >= 0xF900 && c <= 0xFAFF) || // CJK Compatibility Ideographs
            (c >= 0xFE10 && c <= 0xFE19) || // Vertical forms
            (c >= 0xFE30 && c <= 0xFE6F) || // CJK Compatibility Forms
            (c >= 0xFF00 && c <= 0xFF60) || // Fullwidth Forms
            (c >= 0xFFE0 && c <= 0xFFE6) || // Fullwidth currency/symbols
            (c >= 0x2B1B && c <= 0x2B1C)    // ⬛⬜ specifically
        )
            return 2;

        // Supplementary planes (CJK Extension B+, emoji, etc.)
        // char is 16-bit, so surrogate pairs are needed for codepoints > 0xFFFF
        // For lone surrogates, treat as width 1 (not a valid codepoint)

        // Default: width 1
        return 1;
    }
}
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
        // Zero-width: control chars and non-spacing marks
        if (char.IsControl(c) ||
            char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
            return 0;

        int code = c;

        // East Asian Wide / Fullwidth core ranges (W/F)
        if (
            (code >= 0x1100 && code <= 0x115F) ||                 // Hangul Jamo
            (code == 0x2329 || code == 0x232A) ||                 // 〈 〉 angle brackets (W)
            (code >= 0x2E80 && code <= 0xA4CF && code != 0x303F) || // CJK Radicals..Yi (exclude 303F)
            (code >= 0xAC00 && code <= 0xD7A3) ||                 // Hangul Syllables
            (code >= 0xF900 && code <= 0xFAFF) ||                 // CJK Compatibility Ideographs
            (code >= 0xFE10 && code <= 0xFE19) ||                 // Vertical forms
            (code >= 0xFE30 && code <= 0xFE6F) ||                 // CJK compatibility forms
            (code >= 0xFF01 && code <= 0xFF60) ||                 // Fullwidth ASCII variants (F)
            (code >= 0xFFE0 && code <= 0xFFE6)                    // Fullwidth currency/symbols (F)
        )
            return 2;

        // Specific Wide symbols that live outside the big CJK ranges
        // 1) 231A–231B (WATCH, HOURGLASS)
        if (code >= 0x231A && code <= 0x231B) return 2; // W  (watch/hourglass)  :contentReference[oaicite:0]{index=0}

        // 2) Handful of Wide in Misc Symbols (2600–26FF)
        if (
            (code >= 0x26AA && code <= 0x26AB) || // ⚪ ⚫  W  :contentReference[oaicite:1]{index=1}
            (code >= 0x26BD && code <= 0x26BE) || // ⚽ ⚾  W  :contentReference[oaicite:2]{index=2}
            (code >= 0x26C4 && code <= 0x26C5) || // ⛄ ☁? (snowman/sun-behind-cloud) W  
            (code == 0x26EA) ||                   // ⛪  W  
            (code >= 0x26F2 && code <= 0x26F3) || // ⛲ ⛳  W  
            (code == 0x26F5) ||                   // ⛵  W  
            (code == 0x26FA) ||                   // ⛺  W  
            (code == 0x26FD)                      // ⛽  W  :contentReference[oaicite:8]{index=8}
        )
            return 2;

        // 3) Dingbats (2700–27FF): only select ones are Wide
        if (
            (code == 0x2705) ||                   // ✅ WHITE HEAVY CHECK MARK  W  :contentReference[oaicite:9]{index=9}
            (code >= 0x270A && code <= 0x270B) || // ✊ ✋  W  :contentReference[oaicite:10]{index=10}
            (code == 0x2728) ||                   // ✨  W  :contentReference[oaicite:11]{index=11}
            (code == 0x274C) ||                   // ❌  W  :contentReference[oaicite:12]{index=12}
            (code == 0x274E) ||                   // ❎  W  :contentReference[oaicite:13]{index=13}
            (code == 0x27B0)                      // ➰  W  :contentReference[oaicite:14]{index=14}
        )
            return 2;

        // 4) Misc Symbols and Arrows (2B00–2BFF): only a few are Wide
        if (
            (code >= 0x2B1B && code <= 0x2B1C) || // ⬛ ⬜  W  :contentReference[oaicite:15]{index=15}
            (code == 0x2B50) ||                   // ⭐  W  :contentReference[oaicite:16]{index=16}
            (code == 0x2B55)                      // ⭕  W  :contentReference[oaicite:17]{index=17}
        )
            return 2;

        // Everything else: width 1 by default (Ambiguous/Neutral treated as narrow here)
        return 1;
    }

}
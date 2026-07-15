using System.Text;
using System.Collections.Generic;

namespace BookLab.Core.UI
{
    // Minimal Arabic shaper for Unity's built-in UI Text, which draws every letter in its ISOLATED
    // form left-to-right — so Arabic comes out disconnected and reversed. This joins letters into
    // their contextual forms (initial/medial/final), applies lam-alef ligatures, and flips Arabic
    // runs to right-to-left, while keeping Latin/digit runs (authors, dates) left-to-right.
    //
    // Scope: base Arabic letters + lam-alef ligatures — enough for the library's names/authors.
    // (Full harakat/diacritic transparency is a later refinement; the search fields don't use them.)
    // IMPORTANT: this is a DISPLAY transform only — searching/matching always runs on the raw string.
    // Codepoints are written as \u escapes so every glyph is unambiguous.
    public static class ArabicSupport
    {
        // base letter -> (isolated presentation form, isDual). Forms are contiguous in Unicode:
        //   isolated, isolated+1 = final, isolated+2 = initial, isolated+3 = medial.
        // Right-joining letters (dual = false) only really use isolated + final.
        static readonly Dictionary<char, (char iso, bool dual)> Letters = new Dictionary<char, (char iso, bool dual)>
        {
            { 'ء', ('ﺀ', false) }, // hamza (non-joining)
            { 'آ', ('ﺁ', false) }, // alef madda
            { 'أ', ('ﺃ', false) }, // alef hamza above
            { 'ؤ', ('ﺅ', false) }, // waw hamza
            { 'إ', ('ﺇ', false) }, // alef hamza below
            { 'ئ', ('ﺉ', true ) }, // yeh hamza
            { 'ا', ('ﺍ', false) }, // alef
            { 'ب', ('ﺏ', true ) }, // beh
            { 'ة', ('ﺓ', false) }, // teh marbuta
            { 'ت', ('ﺕ', true ) }, // teh
            { 'ث', ('ﺙ', true ) }, // theh
            { 'ج', ('ﺝ', true ) }, // jeem
            { 'ح', ('ﺡ', true ) }, // hah
            { 'خ', ('ﺥ', true ) }, // khah
            { 'د', ('ﺩ', false) }, // dal
            { 'ذ', ('ﺫ', false) }, // thal
            { 'ر', ('ﺭ', false) }, // reh
            { 'ز', ('ﺯ', false) }, // zain
            { 'س', ('ﺱ', true ) }, // seen
            { 'ش', ('ﺵ', true ) }, // sheen
            { 'ص', ('ﺹ', true ) }, // sad
            { 'ض', ('ﺽ', true ) }, // dad
            { 'ط', ('ﻁ', true ) }, // tah
            { 'ظ', ('ﻅ', true ) }, // zah
            { 'ع', ('ﻉ', true ) }, // ain
            { 'غ', ('ﻍ', true ) }, // ghain
            { 'ف', ('ﻑ', true ) }, // feh
            { 'ق', ('ﻕ', true ) }, // qaf
            { 'ك', ('ﻙ', true ) }, // kaf
            { 'ل', ('ﻝ', true ) }, // lam
            { 'م', ('ﻡ', true ) }, // meem
            { 'ن', ('ﻥ', true ) }, // noon
            { 'ه', ('ﻩ', true ) }, // heh
            { 'و', ('ﻭ', false) }, // waw
            { 'ى', ('ﻯ', false) }, // alef maksura
            { 'ي', ('ﻱ', true ) }, // yeh
        };

        // lam ('ل') + alef variant -> isolated ligature (final form = isolated + 1)
        static readonly Dictionary<char, char> LamAlef = new Dictionary<char, char>
        {
            { 'آ', 'ﻵ' }, // lam-alef madda
            { 'أ', 'ﻷ' }, // lam-alef hamza above
            { 'إ', 'ﻹ' }, // lam-alef hamza below
            { 'ا', 'ﻻ' }, // lam-alef
        };

        public static string Fix(string input)
        {
            if (string.IsNullOrEmpty(input) || !ContainsArabic(input)) return input;
            return ReverseForDisplay(Shape(input));
        }

        static bool ContainsArabic(string s)
        {
            foreach (char c in s) if (c >= 'ء' && c <= 'ۿ') return true;
            return false;
        }

        static bool ConnectsBack(char c)    => Letters.ContainsKey(c) && c != 'ء';         // has a final form
        static bool ConnectsForward(char c) => Letters.TryGetValue(c, out var v) && v.dual;     // dual-joining

        static string Shape(string s)
        {
            var outp = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                // Lam-Alef ligature: consumes two characters, emits one.
                if (c == 'ل' && i + 1 < s.Length && LamAlef.TryGetValue(s[i + 1], out char lig))
                {
                    bool lamJoinsPrev = i > 0 && ConnectsForward(s[i - 1]);
                    outp.Append(lamJoinsPrev ? (char)(lig + 1) : lig);   // final : isolated
                    i++;                                                 // skip the alef
                    continue;
                }

                if (Letters.TryGetValue(c, out var info))
                {
                    char prev = i > 0 ? s[i - 1] : '\0';
                    char next = i + 1 < s.Length ? s[i + 1] : '\0';
                    bool joinsPrev = ConnectsForward(prev) && ConnectsBack(c);
                    bool joinsNext = info.dual && ConnectsBack(next);
                    int offset = joinsPrev ? (joinsNext ? 3 : 1)   // medial : final
                                           : (joinsNext ? 2 : 0);  // initial : isolated
                    outp.Append((char)(info.iso + offset));
                }
                else
                {
                    outp.Append(c);   // non-Arabic (Latin, digits, spaces, punctuation) — untouched
                }
            }
            return outp.ToString();
        }

        // Reverse Arabic runs for RTL display, but keep Latin/digit runs left-to-right.
        static string ReverseForDisplay(string s)
        {
            var sb = new StringBuilder(s.Length);
            int i = s.Length - 1;
            while (i >= 0)
            {
                if (IsLtr(s[i]))
                {
                    int j = i;
                    while (j >= 0 && IsLtr(s[j])) j--;
                    sb.Append(s, j + 1, i - j);   // emit the LTR run forward
                    i = j;
                }
                else { sb.Append(s[i]); i--; }
            }
            return sb.ToString();
        }

        static bool IsLtr(char c) =>
            (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
            c == '/' || c == ':' || c == '.' || c == '-';
    }
}

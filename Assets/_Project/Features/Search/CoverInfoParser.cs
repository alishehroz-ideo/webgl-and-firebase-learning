using System.Text.RegularExpressions;

namespace BookLab.Features.Search
{
    // Turns one messy CoverInfo string into a clean ParsedContent — robustly.
    //
    // Real format (reverse-engineered from the live StoryLibary data):
    //     Name _ Author _ Date _ Type _ Subject|Grade|Term _ Tag
    // ...but the field COUNT varies, fields can be empty, tags can contain '_', and RTL Arabic
    // mixes with LTR dates.
    //
    // Strategy: anchor on the DATE by its SHAPE (dd/mm/yyyy), never by position — because the
    // number of fields changes between entries. The author is the field right before the date;
    // the name is everything before the author (re-joined, so a '_' inside a name survives).
    // Everything after the date (type/subject/tag) is extra and ignored. Nothing is hardcoded to
    // any specific entry or id.
    public static class CoverInfoParser
    {
        // dd/mm/yyyy (also tolerates d/m/yyyy). Digits look the same in RTL or LTR, so this
        // anchor holds even in the Arabic entries.
        static readonly Regex DateRx = new Regex(@"^\s*\d{1,2}/\d{1,2}/\d{4}\s*$", RegexOptions.Compiled);

        public static ParsedContent Parse(string raw)
        {
            var result = new ParsedContent { Raw = raw, Confidence = ParseConfidence.Invalid };
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var tokens = raw.Split('_');   // top-level fields

            // 1) find the date by SHAPE, wherever it sits
            int dateIdx = -1;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (DateRx.IsMatch(tokens[i])) { dateIdx = i; break; }
            }

            if (dateIdx >= 0)
            {
                result.Date = tokens[dateIdx].Trim();

                // 2) author = the field right before the date. This needs at least two fields
                //    before it ([name][author][date]). If only ONE field precedes the date, that
                //    field is the name and the author is simply missing.
                if (dateIdx >= 2)
                {
                    result.Author = Clean(tokens[dateIdx - 1]);
                    result.Name   = Clean(string.Join("_", tokens, 0, dateIdx - 1)); // re-join name tokens
                }
                else if (dateIdx == 1)
                {
                    result.Name   = Clean(tokens[0]);
                    result.Author = null;   // no author field present
                }
                // dateIdx == 0 -> nothing before the date: name & author both missing

                result.Confidence = result.HasName ? ParseConfidence.High : ParseConfidence.Low;
            }
            else
            {
                // 3) no date anywhere -> best-effort fallback, flagged low-confidence, never a crash
                result.Name   = tokens.Length > 0 ? Clean(tokens[0]) : null;
                result.Author = tokens.Length > 1 ? Clean(tokens[1]) : null;
                result.Confidence = result.HasName ? ParseConfidence.Low : ParseConfidence.Invalid;
            }

            return result;
        }

        // Trim, and treat an empty/whitespace field as "missing" (null) rather than "".
        static string Clean(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}

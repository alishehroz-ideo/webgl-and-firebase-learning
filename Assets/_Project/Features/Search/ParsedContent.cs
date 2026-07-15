namespace BookLab.Features.Search
{
    // How confident the parser is about a record it pulled out of a messy CoverInfo string.
    public enum ParseConfidence { High, Low, Invalid }

    // The clean, structured result of parsing ONE messy CoverInfo string.
    // We always keep the Raw original and a Confidence flag — nothing is ever lost, and a
    // half-broken entry is marked rather than silently wrong.
    public class ParsedContent
    {
        public string Id;        // the Story id in Firebase (the parent key of the CoverInfo field)
        public string Name;
        public string Author;
        public string Date;      // as found in the data, e.g. "13/04/2025"
        public string Raw;       // the original CoverInfo string, untouched
        public ParseConfidence Confidence;

        public bool HasName   => !string.IsNullOrEmpty(Name);
        public bool HasAuthor => !string.IsNullOrEmpty(Author);
    }
}

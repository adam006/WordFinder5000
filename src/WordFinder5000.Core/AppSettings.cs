namespace WordFinder5000.Core
{
    public class AppSettings
    {
        public HashSetIgnoreCase Excluded { get; set; }
        public int TopWordCount { get; set; }
        public string SourceUrl { get; set; }
    }
}
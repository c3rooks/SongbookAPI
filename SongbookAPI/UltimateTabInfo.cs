public class UltimateTabInfo
{
    public UltimateTabInfo()
    {
    }

    public UltimateTabInfo(string title, string artist, string author, UltimateTab tab, string difficulty = null, string key = null, string capo = null, string tuning = null)
    {
        Title = title;
        Artist = artist;
        Author = author;
        Tab = tab;
        Difficulty = difficulty;
        Key = key;
        Capo = capo;
        Tuning = tuning;
    }

    public string Title { get; set; }
    public string Artist { get; set; }
    public string Author { get; set; }
    public UltimateTab Tab { get; set; }
    public string Difficulty { get; set; }
    public string Key { get; set; }
    public string Capo { get; set; }
    public string Tuning { get; set; }
}

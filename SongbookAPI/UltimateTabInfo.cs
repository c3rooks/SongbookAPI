public class UltimateTabInfo
{

    public string TabUrl { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Author { get; set; }
    public UltimateTab Tab { get; set; }
    public string Difficulty { get; set; }
    public string Key { get; set; }
    public string Capo { get; set; }
    public string Tuning { get; set; }
    public string songName { get; set; }
    public string artistName { get; set; }

public UltimateTabInfo(string songName, string artistName, UltimateTab tab, string author = "UNKNOWN", string difficulty = null, string key = null, string capo = null, string tuning = null, string tabUrl = null)
{
    Title = songName;
    Artist = artistName;
    Author = author;
    Tab = tab;
    Difficulty = difficulty;
    Key = key;
    Capo = capo;
    Tuning = tuning;
    TabUrl = tabUrl;
}

}

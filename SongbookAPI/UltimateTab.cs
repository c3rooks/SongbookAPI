public class UltimateTab
{
    public string Tab { get; set; }
    public string Author { get; set; }
    public string Difficulty { get; set; }
    public string Key { get; set; }
    public string Capo { get; set; }
    public string Tuning { get; set; }
    public string TabUrl { get; set; }
    public string SongName { get; set; }
    public string Title { get; set; }
    public string Tablature { get; set; }
    public string ArtistName { get; set; }
    public List<string> Lines { get; set; } = new List<string>();

    public void AppendBlankLine()
    {
        Lines.Add("");
    }

    public void AppendChordLine(string line)
    {
        Lines.Add("C: " + line);
    }

    public void AppendLyricLine(string line)
    {
        Lines.Add("L: " + line);
    }
}

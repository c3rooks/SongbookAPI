public class UltimateTab
{
    public string Title { get; set; }
    public string Tablature { get; set; }
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

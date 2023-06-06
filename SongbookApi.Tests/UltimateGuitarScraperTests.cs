using SongbookAPI.Scrapers;

public class UltimateGuitarScraperTests
{
    private UltimateGuitarScraper _scraper = new UltimateGuitarScraper();

    [Fact]
    public async Task GetFirstTabUrl_ValidSongAndArtist_ReturnsTabUrl()
    {
        // Arrange
        var songName = "Cold";
        var artistName = "Static-X";

        // Act
        var result = await _scraper.GetFirstTabUrl(songName, artistName);

        // Assert
        Assert.Equal("https://tabs.ultimate-guitar.com/tab/static-x/cold-tabs-24718", result);  // replace ??? with the expected tab URL
    }

    [Fact]
    public async Task GetFirstTabUrl_ValidSongAndArtist_ReturnsTabUrl2()
    {
        // Arrange
        var songName = "A boy brushed red living in black and white";
        var artistName = "underoath";

        // Act
        var result = await _scraper.GetFirstTabUrl(songName, artistName);

        // Assert
        Assert.Equal("https://tabs.ultimate-guitar.com/tab/underoath/a-boy-brushed-red-living-in-black-and-white-tabs-177990", result);  // replace ??? with the expected tab URL
    }

    [Fact]
    public async Task GetFirstTabUrl_InvalidSong_ReturnsNull()
    {
        // Arrange
        var songName = "Pittsburgh";
        var artistName = "Mayday Parade";

        // Act
        var result = await _scraper.GetFirstTabUrl(songName, artistName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFirstTabUrl_InvalidArtist_ReturnsNull()
    {
        // Arrange
        var songName = "Dogma";
        var artistName = "Led Zeppelin";

        // Act
        var result = await _scraper.GetFirstTabUrl(songName, artistName);

        // Assert
        Assert.Null(result);
    }
}

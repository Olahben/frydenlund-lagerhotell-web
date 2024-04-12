namespace Lagerhotell.Services;

public class TextHandler
{

    /// <summary>
    /// Parses a string into paragraphs
    /// </summary>
    /// <param name="text"></param>
    /// <returns>list of paragraphs</returns>
    public List<string> ParseParagraphs(string text)
    {
        List<string> paragraphs = new List<string>();
        string[] lines = text.Split("\n\n");
        foreach (string line in lines)
        {
            paragraphs.Add(line);
        }
        return paragraphs;
    }

    public List<string> ParseBulletPoints(string text)
    {
        List<string> paragraphs = new List<string>();
        string[] lines = text.Split("\n");
        foreach (string line in lines)
        {
            paragraphs.Add(line);
        }
        return paragraphs;
    }
}

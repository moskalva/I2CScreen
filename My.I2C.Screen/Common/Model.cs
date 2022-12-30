
public record struct ScreenSize(uint Horizontal, uint Vertical);
public enum SectionAddresingMode { Horizontal }

public record struct ScreenSectionPosition(uint Row, uint Column)
{
    public static readonly ScreenSectionPosition Zero = new ScreenSectionPosition(0, 0);
}

public record struct ScreenSection(ScreenSectionPosition Position, ScreenSectionData Data);
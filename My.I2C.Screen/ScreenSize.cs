

public record struct ScreenSize (uint Horizontal, uint Vertical);

public record struct ScreenSection(
    uint StartPage,
    uint EndPage,
    uint StartColumn,
    uint Width
);
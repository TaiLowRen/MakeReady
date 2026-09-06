namespace MakeReady.Models;

public class Magazine
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    public Firearm Firearm { get; set; } = null!;
    public string Label { get; set; } = "";
    public int Capacity { get; set; }
    public int TotalRoundsFired { get; set; }

    public List<MagazineModification> Modifications { get; set; } = new();
    public List<MagazineMalfunction> Malfunctions { get; set; } = new();

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? $"{Capacity}rd mag" : Label;
    public double MalfunctionRate => TotalRoundsFired > 0 ? (double)Malfunctions.Count / TotalRoundsFired * 100 : 0;
}

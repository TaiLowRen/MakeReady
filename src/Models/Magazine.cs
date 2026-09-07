using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class Magazine
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    [JsonIgnore] public Firearm Firearm { get; set; } = null!;
    public string Label { get; set; } = "";
    public int Capacity { get; set; }
    public int TotalRoundsFired { get; set; }

    [JsonIgnore] public List<MagazineModification> Modifications { get; set; } = new();
    [JsonIgnore] public List<MagazineMalfunction> Malfunctions { get; set; } = new();

    [JsonIgnore] public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? $"{Capacity}rd mag" : Label;
    [JsonIgnore] public double MalfunctionRate => TotalRoundsFired > 0 ? (double)Malfunctions.Count / TotalRoundsFired * 100 : 0;
}

using System.Text.Json.Serialization;

namespace MakeReady.Models;

public enum TargetShape { Circle, Square, Silhouette }

public enum TargetMovement { Static, LeftToRight, RightToLeft, TopToBottom, BottomToTop }

// A single exposure event in a drill's timeline: appears at AppearAtSeconds,
// stays visible for DurationSeconds, optionally sweeping across the screen.
// For a moving target, PositionXPercent/PositionYPercent is the fixed
// cross-axis coordinate (e.g. the row height for a left/right mover) --
// the travel axis always spans the full width/height of the runner surface.
public class DrillTarget
{
    public int Id { get; set; }
    public int DrillId { get; set; }
    [JsonIgnore] public Drill Drill { get; set; } = null!;

    public int OrderIndex { get; set; }
    public TargetShape Shape { get; set; } = TargetShape.Circle;
    public TargetMovement Movement { get; set; } = TargetMovement.Static;

    public double SizePercent { get; set; } = 18;
    public double PositionXPercent { get; set; } = 50;
    public double PositionYPercent { get; set; } = 50;

    public double AppearAtSeconds { get; set; } = 0;
    public double DurationSeconds { get; set; } = 2;

    [JsonIgnore]
    public string ShapeLabel => Shape switch
    {
        TargetShape.Circle => "Circle",
        TargetShape.Square => "Square",
        _ => "Silhouette"
    };

    [JsonIgnore]
    public string MovementLabel => Movement switch
    {
        TargetMovement.LeftToRight => "Left → Right",
        TargetMovement.RightToLeft => "Right → Left",
        TargetMovement.TopToBottom => "Top → Bottom",
        TargetMovement.BottomToTop => "Bottom → Top",
        _ => "Static"
    };
}

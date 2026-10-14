namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan checkpoint is in its life cycle.</summary>
public enum FloorPlanCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A floor plan checkpoint as the floor plan team keeps it.</summary>
public sealed class FloorPlanCheckpoint(string code, string label, string segment, string description, int version, int updatedAt, double weight, FloorPlanCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Weight { get; set; } = weight;
    public FloorPlanCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan checkpoint in status draft.</summary>
    public static FloorPlanCheckpoint NewFloorPlanCheckpoint(string code, string label)
    {
        return new FloorPlanCheckpoint(code, label, "", "", 0, 0, 0.0, FloorPlanCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanCheckpointStatus next)
    {
        switch (Status)
        {
            case FloorPlanCheckpointStatus.Draft:
                return next == FloorPlanCheckpointStatus.Active;
            case FloorPlanCheckpointStatus.Pending:
                switch (next)
                {
                    case FloorPlanCheckpointStatus.Active:
                    case FloorPlanCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case FloorPlanCheckpointStatus.Active:
                switch (next)
                {
                    case FloorPlanCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case FloorPlanCheckpointStatus.Suspended:
                return next == FloorPlanCheckpointStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FloorPlanCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"floor plan checkpoint"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Weight + (double)UpdatedAt, 2);
    }

    public FloorPlanCheckpoint Copy()
    {
        return new FloorPlanCheckpoint(Code, Label, Segment, Description, Version, UpdatedAt, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

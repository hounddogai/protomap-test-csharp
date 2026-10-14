namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan checkpoint is in its life cycle.</summary>
public enum SeasonalFloorPlanCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal floor plan checkpoint. Its code is unique within a tenant.</summary>
public sealed class SeasonalFloorPlanCheckpoint(string code, string name, double ceiling, double score, string employeeId, double maximum, int quantity, string notes, int createdAt, double discount, bool active, SeasonalFloorPlanCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public string Notes { get; set; } = notes;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public bool Active { get; set; } = active;
    public SeasonalFloorPlanCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan checkpoint in status idle.</summary>
    public static SeasonalFloorPlanCheckpoint NewSeasonalFloorPlanCheckpoint(string code, string name)
    {
        return new SeasonalFloorPlanCheckpoint(
            code,
            name,
            0.0,
            0.0,
            "",
            0.0,
            0,
            "",
            0,
            0.0,
            false,
            SeasonalFloorPlanCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanCheckpointStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanCheckpointStatus.Idle:
                return next == SeasonalFloorPlanCheckpointStatus.Draining || next == SeasonalFloorPlanCheckpointStatus.Stopped;
            case SeasonalFloorPlanCheckpointStatus.Warming:
                return next == SeasonalFloorPlanCheckpointStatus.Draining;
            case SeasonalFloorPlanCheckpointStatus.Ready:
                return next == SeasonalFloorPlanCheckpointStatus.Draining || next == SeasonalFloorPlanCheckpointStatus.Stopped;
            case SeasonalFloorPlanCheckpointStatus.Draining:
                return next == SeasonalFloorPlanCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Discount - (double)Quantity, 2);
    }

    public SeasonalFloorPlanCheckpoint Copy()
    {
        return new SeasonalFloorPlanCheckpoint(
            Code,
            Name,
            Ceiling,
            Score,
            EmployeeId,
            Maximum,
            Quantity,
            Notes,
            CreatedAt,
            Discount,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

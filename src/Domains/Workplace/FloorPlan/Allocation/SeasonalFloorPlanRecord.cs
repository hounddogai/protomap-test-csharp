namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan record is in its life cycle.</summary>
public enum SeasonalFloorPlanRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One seasonal floor plan record, with what the planners need to act on it.</summary>
public sealed class SeasonalFloorPlanRecord(string code, string label, string unit, string description, int startsAt, bool approved, string location, string badge, SeasonalFloorPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public string Badge { get; set; } = badge;
    public SeasonalFloorPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan record in status open.</summary>
    public static SeasonalFloorPlanRecord NewSeasonalFloorPlanRecord(string code, string label)
    {
        return new SeasonalFloorPlanRecord(
            code,
            label,
            "",
            "",
            0,
            false,
            "",
            "",
            SeasonalFloorPlanRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanRecordStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanRecordStatus.Open:
                return next == SeasonalFloorPlanRecordStatus.Resolved || next == SeasonalFloorPlanRecordStatus.Archived;
            case SeasonalFloorPlanRecordStatus.InReview:
                switch (next)
                {
                    case SeasonalFloorPlanRecordStatus.Resolved:
                        return true;
                }
                return false;
            case SeasonalFloorPlanRecordStatus.Resolved:
                return next == SeasonalFloorPlanRecordStatus.Reopened || next == SeasonalFloorPlanRecordStatus.Archived;
            case SeasonalFloorPlanRecordStatus.Reopened:
                switch (next)
                {
                    case SeasonalFloorPlanRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"seasonal floor plan record"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public SeasonalFloorPlanRecord Copy()
    {
        return new SeasonalFloorPlanRecord(
            Code,
            Label,
            Unit,
            Description,
            StartsAt,
            Approved,
            Location,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}

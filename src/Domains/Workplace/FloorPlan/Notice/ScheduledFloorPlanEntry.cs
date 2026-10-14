namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled floor plan entry is in its life cycle.</summary>
public enum ScheduledFloorPlanEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One scheduled floor plan entry, with what the planners need to act on it.</summary>
public sealed class ScheduledFloorPlanEntry(string code, string title, bool active, int endsAt, string skill, bool archived, string segment, double weight, string costCenter, int expiresAt, int tenureMonths, ScheduledFloorPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public double Weight { get; set; } = weight;
    public string CostCenter { get; set; } = costCenter;
    public int ExpiresAt { get; set; } = expiresAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledFloorPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled floor plan entry in status requested.</summary>
    public static ScheduledFloorPlanEntry NewScheduledFloorPlanEntry(string code, string title)
    {
        return new ScheduledFloorPlanEntry(
            code,
            title,
            false,
            0,
            "",
            false,
            "",
            0.0,
            "",
            0,
            0,
            ScheduledFloorPlanEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFloorPlanEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFloorPlanEntryStatus next)
    {
        switch (Status)
        {
            case ScheduledFloorPlanEntryStatus.Requested:
                return next == ScheduledFloorPlanEntryStatus.Granted || next == ScheduledFloorPlanEntryStatus.Expired;
            case ScheduledFloorPlanEntryStatus.Granted:
                return next == ScheduledFloorPlanEntryStatus.Revoked || next == ScheduledFloorPlanEntryStatus.Expired;
            case ScheduledFloorPlanEntryStatus.Revoked:
                switch (next)
                {
                    case ScheduledFloorPlanEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledFloorPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled floor plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedValue()
    {
        return Math.Round(Weight + (double)EndsAt, 2);
    }

    public ScheduledFloorPlanEntry Copy()
    {
        return new ScheduledFloorPlanEntry(
            Code,
            Title,
            Active,
            EndsAt,
            Skill,
            Archived,
            Segment,
            Weight,
            CostCenter,
            ExpiresAt,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

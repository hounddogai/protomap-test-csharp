namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan bundle is in its life cycle.</summary>
public enum RollingFloorPlanBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling floor plan bundle as the floor plan team keeps it.</summary>
public sealed class RollingFloorPlanBundle(string code, string label, int startsAt, string reviewer, double fte, string badge, string team, int endsAt, int capacity, string currency, int priority, RollingFloorPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public int Priority { get; set; } = priority;
    public RollingFloorPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan bundle in status planned.</summary>
    public static RollingFloorPlanBundle NewRollingFloorPlanBundle(string code, string label)
    {
        return new RollingFloorPlanBundle(
            code,
            label,
            0,
            "",
            0.0,
            "",
            "",
            0,
            0,
            "",
            0,
            RollingFloorPlanBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanBundleStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanBundleStatus.Planned:
                switch (next)
                {
                    case RollingFloorPlanBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingFloorPlanBundleStatus.Scheduled:
                switch (next)
                {
                    case RollingFloorPlanBundleStatus.Completed:
                    case RollingFloorPlanBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingFloorPlanBundleStatus.InProgress:
                return next == RollingFloorPlanBundleStatus.Completed || next == RollingFloorPlanBundleStatus.Cancelled;
            case RollingFloorPlanBundleStatus.Completed:
                return next == RollingFloorPlanBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)EndsAt * (double)StartsAt, 2);
    }

    public RollingFloorPlanBundle Copy()
    {
        return new RollingFloorPlanBundle(
            Code,
            Label,
            StartsAt,
            Reviewer,
            Fte,
            Badge,
            Team,
            EndsAt,
            Capacity,
            Currency,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

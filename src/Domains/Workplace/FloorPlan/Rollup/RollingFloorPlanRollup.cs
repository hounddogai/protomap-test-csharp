namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan rollup is in its life cycle.</summary>
public enum RollingFloorPlanRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rolling floor plan rollup, with what the planners need to act on it.</summary>
public sealed class RollingFloorPlanRollup(string code, string name, double margin, double maximum, double floorValue, double rating, int limit, RollingFloorPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public RollingFloorPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan rollup in status proposed.</summary>
    public static RollingFloorPlanRollup NewRollingFloorPlanRollup(string code, string name)
    {
        return new RollingFloorPlanRollup(code, name, 0.0, 0.0, 0.0, 0.0, 0, RollingFloorPlanRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanRollupStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanRollupStatus.Proposed:
                switch (next)
                {
                    case RollingFloorPlanRollupStatus.Rejected:
                        return true;
                }
                return false;
            case RollingFloorPlanRollupStatus.Approved:
                switch (next)
                {
                    case RollingFloorPlanRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingFloorPlanRollupStatus.Rejected:
                return next == RollingFloorPlanRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Maximum + (double)Limit, 2);
    }

    public RollingFloorPlanRollup Copy()
    {
        return new RollingFloorPlanRollup(Code, Name, Margin, Maximum, FloorValue, Rating, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

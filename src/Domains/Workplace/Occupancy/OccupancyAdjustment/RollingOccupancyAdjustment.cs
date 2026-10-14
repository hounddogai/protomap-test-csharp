namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling occupancy adjustment is in its life cycle.</summary>
public enum RollingOccupancyAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling occupancy adjustment as the occupancy team keeps it.</summary>
public sealed class RollingOccupancyAdjustment(string code, string label, int version, double ratio, string region, int quantity, RollingOccupancyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public RollingOccupancyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling occupancy adjustment in status open.</summary>
    public static RollingOccupancyAdjustment NewRollingOccupancyAdjustment(string code, string label)
    {
        return new RollingOccupancyAdjustment(code, label, 0, 0.0, "", 0, RollingOccupancyAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingOccupancyAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingOccupancyAdjustmentStatus next)
    {
        switch (Status)
        {
            case RollingOccupancyAdjustmentStatus.Open:
                switch (next)
                {
                    case RollingOccupancyAdjustmentStatus.Reopened:
                    case RollingOccupancyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case RollingOccupancyAdjustmentStatus.InReview:
                switch (next)
                {
                    case RollingOccupancyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case RollingOccupancyAdjustmentStatus.Resolved:
                return next == RollingOccupancyAdjustmentStatus.Reopened || next == RollingOccupancyAdjustmentStatus.Archived;
            case RollingOccupancyAdjustmentStatus.Reopened:
                switch (next)
                {
                    case RollingOccupancyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingOccupancyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling occupancy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"rolling occupancy adjustment"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Version * (double)Quantity, 2);
    }

    public RollingOccupancyAdjustment Copy()
    {
        return new RollingOccupancyAdjustment(Code, Label, Version, Ratio, Region, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}

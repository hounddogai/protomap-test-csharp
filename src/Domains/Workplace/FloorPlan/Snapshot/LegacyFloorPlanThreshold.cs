namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy floor plan threshold is in its life cycle.</summary>
public enum LegacyFloorPlanThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy floor plan threshold. Its code is unique within a tenant.</summary>
public sealed class LegacyFloorPlanThreshold(string code, string title, double minimum, string category, int quantity, bool pinned, double fte, string badge, LegacyFloorPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public LegacyFloorPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a legacy floor plan threshold in status requested.</summary>
    public static LegacyFloorPlanThreshold NewLegacyFloorPlanThreshold(string code, string title)
    {
        return new LegacyFloorPlanThreshold(
            code,
            title,
            0.0,
            "",
            0,
            false,
            0.0,
            "",
            LegacyFloorPlanThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyFloorPlanThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyFloorPlanThresholdStatus next)
    {
        switch (Status)
        {
            case LegacyFloorPlanThresholdStatus.Requested:
                switch (next)
                {
                    case LegacyFloorPlanThresholdStatus.Revoked:
                        return true;
                }
                return false;
            case LegacyFloorPlanThresholdStatus.Granted:
                return next == LegacyFloorPlanThresholdStatus.Revoked;
            case LegacyFloorPlanThresholdStatus.Revoked:
                switch (next)
                {
                    case LegacyFloorPlanThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyFloorPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy floor plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy floor plan threshold"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Fte + Minimum, 2);
    }

    public LegacyFloorPlanThreshold Copy()
    {
        return new LegacyFloorPlanThreshold(Code, Title, Minimum, Category, Quantity, Pinned, Fte, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}

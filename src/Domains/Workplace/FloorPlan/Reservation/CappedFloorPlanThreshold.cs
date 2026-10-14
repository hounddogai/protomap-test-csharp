namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped floor plan threshold is in its life cycle.</summary>
public enum CappedFloorPlanThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A capped floor plan threshold as the floor plan team keeps it.</summary>
public sealed class CappedFloorPlanThreshold(string code, string label, double cost, string phone, int attempts, bool visible, string ownerId, string segment, double maximum, string source, int tenureMonths, CappedFloorPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Phone { get; set; } = phone;
    public int Attempts { get; set; } = attempts;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public string Segment { get; set; } = segment;
    public double Maximum { get; set; } = maximum;
    public string Source { get; set; } = source;
    public int TenureMonths { get; set; } = tenureMonths;
    public CappedFloorPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped floor plan threshold in status proposed.</summary>
    public static CappedFloorPlanThreshold NewCappedFloorPlanThreshold(string code, string label)
    {
        return new CappedFloorPlanThreshold(
            code,
            label,
            0.0,
            "",
            0,
            false,
            "",
            "",
            0.0,
            "",
            0,
            CappedFloorPlanThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedFloorPlanThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedFloorPlanThresholdStatus next)
    {
        switch (Status)
        {
            case CappedFloorPlanThresholdStatus.Proposed:
                switch (next)
                {
                    case CappedFloorPlanThresholdStatus.Approved:
                    case CappedFloorPlanThresholdStatus.Rejected:
                        return true;
                }
                return false;
            case CappedFloorPlanThresholdStatus.Approved:
                switch (next)
                {
                    case CappedFloorPlanThresholdStatus.Rejected:
                        return true;
                }
                return false;
            case CappedFloorPlanThresholdStatus.Rejected:
                return next == CappedFloorPlanThresholdStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedFloorPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped floor plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Attempts + (double)TenureMonths, 2);
    }

    public CappedFloorPlanThreshold Copy()
    {
        return new CappedFloorPlanThreshold(
            Code,
            Label,
            Cost,
            Phone,
            Attempts,
            Visible,
            OwnerId,
            Segment,
            Maximum,
            Source,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

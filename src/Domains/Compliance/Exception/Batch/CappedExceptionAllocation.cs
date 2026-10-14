namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped exception allocation is in its life cycle.</summary>
public enum CappedExceptionAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a capped exception allocation and its current status.</summary>
public sealed class CappedExceptionAllocation(string code, string label, double weight, int tenureMonths, double fte, string location, int durationMinutes, double cost, CappedExceptionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Cost { get; set; } = cost;
    public CappedExceptionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a capped exception allocation in status open.</summary>
    public static CappedExceptionAllocation NewCappedExceptionAllocation(string code, string label)
    {
        return new CappedExceptionAllocation(
            code,
            label,
            0.0,
            0,
            0.0,
            "",
            0,
            0.0,
            CappedExceptionAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedExceptionAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedExceptionAllocationStatus next)
    {
        switch (Status)
        {
            case CappedExceptionAllocationStatus.Open:
                switch (next)
                {
                    case CappedExceptionAllocationStatus.Resolved:
                    case CappedExceptionAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case CappedExceptionAllocationStatus.InReview:
                switch (next)
                {
                    case CappedExceptionAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case CappedExceptionAllocationStatus.Resolved:
                return next == CappedExceptionAllocationStatus.Reopened;
            case CappedExceptionAllocationStatus.Reopened:
                return next == CappedExceptionAllocationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedExceptionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped exception allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight + Cost, 2);
    }

    public CappedExceptionAllocation Copy()
    {
        return new CappedExceptionAllocation(
            Code,
            Label,
            Weight,
            TenureMonths,
            Fte,
            Location,
            DurationMinutes,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

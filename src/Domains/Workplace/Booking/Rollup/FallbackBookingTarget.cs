namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback booking target is in its life cycle.</summary>
public enum FallbackBookingTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a fallback booking target and its current status.</summary>
public sealed class FallbackBookingTarget(string code, string name, int windowDays, double threshold, int quantity, bool approved, int tenureMonths, string checksum, FallbackBookingTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Checksum { get; set; } = checksum;
    public FallbackBookingTargetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback booking target in status open.</summary>
    public static FallbackBookingTarget NewFallbackBookingTarget(string code, string name)
    {
        return new FallbackBookingTarget(code, name, 0, 0.0, 0, false, 0, "", FallbackBookingTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBookingTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBookingTargetStatus next)
    {
        switch (Status)
        {
            case FallbackBookingTargetStatus.Open:
                return next == FallbackBookingTargetStatus.InReview;
            case FallbackBookingTargetStatus.InReview:
                switch (next)
                {
                    case FallbackBookingTargetStatus.Archived:
                        return true;
                }
                return false;
            case FallbackBookingTargetStatus.Resolved:
                return next == FallbackBookingTargetStatus.Archived;
            case FallbackBookingTargetStatus.Reopened:
                return next == FallbackBookingTargetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackBookingTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback booking target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback booking target"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Threshold - (double)TenureMonths, 2);
    }

    public FallbackBookingTarget Copy()
    {
        return new FallbackBookingTarget(
            Code,
            Name,
            WindowDays,
            Threshold,
            Quantity,
            Approved,
            TenureMonths,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

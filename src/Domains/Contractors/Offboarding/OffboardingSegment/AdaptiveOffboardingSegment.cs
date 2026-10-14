namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive offboarding segment is in its life cycle.</summary>
public enum AdaptiveOffboardingSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive offboarding segment as the offboarding team keeps it.</summary>
public sealed class AdaptiveOffboardingSegment(string code, string title, double margin, double amount, int windowDays, AdaptiveOffboardingSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public double Amount { get; set; } = amount;
    public int WindowDays { get; set; } = windowDays;
    public AdaptiveOffboardingSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive offboarding segment in status planned.</summary>
    public static AdaptiveOffboardingSegment NewAdaptiveOffboardingSegment(string code, string title)
    {
        return new AdaptiveOffboardingSegment(code, title, 0.0, 0.0, 0, AdaptiveOffboardingSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveOffboardingSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveOffboardingSegmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveOffboardingSegmentStatus.Planned:
                return next == AdaptiveOffboardingSegmentStatus.Cancelled;
            case AdaptiveOffboardingSegmentStatus.Scheduled:
                return next == AdaptiveOffboardingSegmentStatus.Cancelled;
            case AdaptiveOffboardingSegmentStatus.InProgress:
                return next == AdaptiveOffboardingSegmentStatus.Completed || next == AdaptiveOffboardingSegmentStatus.Cancelled;
            case AdaptiveOffboardingSegmentStatus.Completed:
                return next == AdaptiveOffboardingSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveOffboardingSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive offboarding segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive offboarding segment"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)WindowDays * Amount, 2);
    }

    public AdaptiveOffboardingSegment Copy()
    {
        return new AdaptiveOffboardingSegment(Code, Title, Margin, Amount, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

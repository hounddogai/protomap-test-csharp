namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive deduction baseline is in its life cycle.</summary>
public enum AdaptiveDeductionBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive deduction baseline as the deduction team keeps it.</summary>
public sealed class AdaptiveDeductionBaseline(string code, string title, double cost, bool visible, double rating, int durationMinutes, AdaptiveDeductionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public bool Visible { get; set; } = visible;
    public double Rating { get; set; } = rating;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AdaptiveDeductionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive deduction baseline in status open.</summary>
    public static AdaptiveDeductionBaseline NewAdaptiveDeductionBaseline(string code, string title)
    {
        return new AdaptiveDeductionBaseline(code, title, 0.0, false, 0.0, 0, AdaptiveDeductionBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveDeductionBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveDeductionBaselineStatus next)
    {
        switch (Status)
        {
            case AdaptiveDeductionBaselineStatus.Open:
                return next == AdaptiveDeductionBaselineStatus.InReview || next == AdaptiveDeductionBaselineStatus.Reopened;
            case AdaptiveDeductionBaselineStatus.InReview:
                return next == AdaptiveDeductionBaselineStatus.Resolved;
            case AdaptiveDeductionBaselineStatus.Resolved:
                return next == AdaptiveDeductionBaselineStatus.Archived;
            case AdaptiveDeductionBaselineStatus.Reopened:
                return next == AdaptiveDeductionBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveDeductionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive deduction baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost * Rating, 2);
    }

    public AdaptiveDeductionBaseline Copy()
    {
        return new AdaptiveDeductionBaseline(Code, Title, Cost, Visible, Rating, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

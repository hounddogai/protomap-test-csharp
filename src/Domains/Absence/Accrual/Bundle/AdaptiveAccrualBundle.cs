namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive accrual bundle is in its life cycle.</summary>
public enum AdaptiveAccrualBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive accrual bundle as the accrual team keeps it.</summary>
public sealed class AdaptiveAccrualBundle(string code, string label, string salaryBand, double ratio, int startsAt, int createdAt, string badge, int attempts, string checksum, int durationMinutes, AdaptiveAccrualBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public int Attempts { get; set; } = attempts;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AdaptiveAccrualBundleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive accrual bundle in status open.</summary>
    public static AdaptiveAccrualBundle NewAdaptiveAccrualBundle(string code, string label)
    {
        return new AdaptiveAccrualBundle(code, label, "", 0.0, 0, 0, "", 0, "", 0, AdaptiveAccrualBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAccrualBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAccrualBundleStatus next)
    {
        switch (Status)
        {
            case AdaptiveAccrualBundleStatus.Open:
                return next == AdaptiveAccrualBundleStatus.InReview || next == AdaptiveAccrualBundleStatus.Reopened;
            case AdaptiveAccrualBundleStatus.InReview:
                return next == AdaptiveAccrualBundleStatus.Reopened;
            case AdaptiveAccrualBundleStatus.Resolved:
                return next == AdaptiveAccrualBundleStatus.Reopened;
            case AdaptiveAccrualBundleStatus.Reopened:
                return next == AdaptiveAccrualBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveAccrualBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive accrual bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)CreatedAt - (double)DurationMinutes, 2);
    }

    public AdaptiveAccrualBundle Copy()
    {
        return new AdaptiveAccrualBundle(
            Code,
            Label,
            SalaryBand,
            Ratio,
            StartsAt,
            CreatedAt,
            Badge,
            Attempts,
            Checksum,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

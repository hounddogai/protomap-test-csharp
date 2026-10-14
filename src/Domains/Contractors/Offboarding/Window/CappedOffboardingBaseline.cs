namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped offboarding baseline is in its life cycle.</summary>
public enum CappedOffboardingBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a capped offboarding baseline and its current status.</summary>
public sealed class CappedOffboardingBaseline(string code, string title, int windowDays, string segment, double rating, double amount, int batchSize, string checksum, CappedOffboardingBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public int BatchSize { get; set; } = batchSize;
    public string Checksum { get; set; } = checksum;
    public CappedOffboardingBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a capped offboarding baseline in status draft.</summary>
    public static CappedOffboardingBaseline NewCappedOffboardingBaseline(string code, string title)
    {
        return new CappedOffboardingBaseline(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            0,
            "",
            CappedOffboardingBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOffboardingBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOffboardingBaselineStatus next)
    {
        switch (Status)
        {
            case CappedOffboardingBaselineStatus.Draft:
                return next == CappedOffboardingBaselineStatus.Suspended;
            case CappedOffboardingBaselineStatus.Pending:
                return next == CappedOffboardingBaselineStatus.Closed;
            case CappedOffboardingBaselineStatus.Active:
                return next == CappedOffboardingBaselineStatus.Closed;
            case CappedOffboardingBaselineStatus.Suspended:
                return next == CappedOffboardingBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedOffboardingBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped offboarding baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize + Amount, 2);
    }

    public CappedOffboardingBaseline Copy()
    {
        return new CappedOffboardingBaseline(
            Code,
            Title,
            WindowDays,
            Segment,
            Rating,
            Amount,
            BatchSize,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

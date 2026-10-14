namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual threshold is in its life cycle.</summary>
public enum AccrualThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a accrual threshold and its current status.</summary>
public sealed class AccrualThreshold(string code, string name, string title, string currency, double score, int tenureMonths, double ceiling, int expiresAt, string reviewer, string label, string region, AccrualThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Score { get; set; } = score;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public AccrualThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a accrual threshold in status draft.</summary>
    public static AccrualThreshold NewAccrualThreshold(string code, string name)
    {
        return new AccrualThreshold(code, name, "", "", 0.0, 0, 0.0, 0, "", "", "", AccrualThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualThresholdStatus next)
    {
        switch (Status)
        {
            case AccrualThresholdStatus.Draft:
                return next == AccrualThresholdStatus.Suspended || next == AccrualThresholdStatus.Closed;
            case AccrualThresholdStatus.Pending:
                switch (next)
                {
                    case AccrualThresholdStatus.Active:
                    case AccrualThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case AccrualThresholdStatus.Active:
                return next == AccrualThresholdStatus.Suspended || next == AccrualThresholdStatus.Closed;
            case AccrualThresholdStatus.Suspended:
                return next == AccrualThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccrualThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"accrual threshold"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt * Score, 2);
    }

    public AccrualThreshold Copy()
    {
        return new AccrualThreshold(
            Code,
            Name,
            Title,
            Currency,
            Score,
            TenureMonths,
            Ceiling,
            ExpiresAt,
            Reviewer,
            Label,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}

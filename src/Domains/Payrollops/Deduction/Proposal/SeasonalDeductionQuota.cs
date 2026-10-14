namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal deduction quota is in its life cycle.</summary>
public enum SeasonalDeductionQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One seasonal deduction quota, with what the planners need to act on it.</summary>
public sealed class SeasonalDeductionQuota(string code, string label, string title, int expiresAt, int version, double discount, string channel, string source, double rate, int batchSize, double amount, SeasonalDeductionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public double Discount { get; set; } = discount;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public int BatchSize { get; set; } = batchSize;
    public double Amount { get; set; } = amount;
    public SeasonalDeductionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal deduction quota in status planned.</summary>
    public static SeasonalDeductionQuota NewSeasonalDeductionQuota(string code, string label)
    {
        return new SeasonalDeductionQuota(
            code,
            label,
            "",
            0,
            0,
            0.0,
            "",
            "",
            0.0,
            0,
            0.0,
            SeasonalDeductionQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalDeductionQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalDeductionQuotaStatus next)
    {
        switch (Status)
        {
            case SeasonalDeductionQuotaStatus.Planned:
                switch (next)
                {
                    case SeasonalDeductionQuotaStatus.Completed:
                        return true;
                }
                return false;
            case SeasonalDeductionQuotaStatus.Scheduled:
                switch (next)
                {
                    case SeasonalDeductionQuotaStatus.InProgress:
                        return true;
                }
                return false;
            case SeasonalDeductionQuotaStatus.InProgress:
                return next == SeasonalDeductionQuotaStatus.Completed || next == SeasonalDeductionQuotaStatus.Cancelled;
            case SeasonalDeductionQuotaStatus.Completed:
                return next == SeasonalDeductionQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalDeductionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal deduction quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Version + (double)ExpiresAt, 2);
    }

    public SeasonalDeductionQuota Copy()
    {
        return new SeasonalDeductionQuota(
            Code,
            Label,
            Title,
            ExpiresAt,
            Version,
            Discount,
            Channel,
            Source,
            Rate,
            BatchSize,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

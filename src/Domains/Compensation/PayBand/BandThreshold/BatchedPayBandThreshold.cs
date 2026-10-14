namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pay band threshold is in its life cycle.</summary>
public enum BatchedPayBandThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched pay band threshold. Its code is unique within a tenant.</summary>
public sealed class BatchedPayBandThreshold(string code, string name, string country, double fte, double margin, string unit, string locale, string channel, int priority, string source, string reviewer, BatchedPayBandThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public string Unit { get; set; } = unit;
    public string Locale { get; set; } = locale;
    public string Channel { get; set; } = channel;
    public int Priority { get; set; } = priority;
    public string Source { get; set; } = source;
    public string Reviewer { get; set; } = reviewer;
    public BatchedPayBandThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a batched pay band threshold in status requested.</summary>
    public static BatchedPayBandThreshold NewBatchedPayBandThreshold(string code, string name)
    {
        return new BatchedPayBandThreshold(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            0,
            "",
            "",
            BatchedPayBandThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayBandThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayBandThresholdStatus next)
    {
        switch (Status)
        {
            case BatchedPayBandThresholdStatus.Requested:
                return next == BatchedPayBandThresholdStatus.Expired;
            case BatchedPayBandThresholdStatus.Granted:
                return next == BatchedPayBandThresholdStatus.Revoked || next == BatchedPayBandThresholdStatus.Expired;
            case BatchedPayBandThresholdStatus.Revoked:
                switch (next)
                {
                    case BatchedPayBandThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayBandThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pay band threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched pay band threshold"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Fte - (double)Priority, 2);
    }

    public BatchedPayBandThreshold Copy()
    {
        return new BatchedPayBandThreshold(
            Code,
            Name,
            Country,
            Fte,
            Margin,
            Unit,
            Locale,
            Channel,
            Priority,
            Source,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

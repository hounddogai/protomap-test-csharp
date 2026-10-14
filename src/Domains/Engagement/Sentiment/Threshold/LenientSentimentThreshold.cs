namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient sentiment threshold is in its life cycle.</summary>
public enum LenientSentimentThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient sentiment threshold. Its code is unique within a tenant.</summary>
public sealed class LenientSentimentThreshold(string code, string label, string salaryBand, string country, int graceDays, double amount, double floorValue, string category, int durationMinutes, string timezone, LenientSentimentThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Country { get; set; } = country;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Timezone { get; set; } = timezone;
    public LenientSentimentThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a lenient sentiment threshold in status draft.</summary>
    public static LenientSentimentThreshold NewLenientSentimentThreshold(string code, string label)
    {
        return new LenientSentimentThreshold(
            code,
            label,
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            0,
            "",
            LenientSentimentThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSentimentThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSentimentThresholdStatus next)
    {
        switch (Status)
        {
            case LenientSentimentThresholdStatus.Draft:
                return next == LenientSentimentThresholdStatus.Active || next == LenientSentimentThresholdStatus.Closed;
            case LenientSentimentThresholdStatus.Pending:
                switch (next)
                {
                    case LenientSentimentThresholdStatus.Closed:
                        return true;
                }
                return false;
            case LenientSentimentThresholdStatus.Active:
                return next == LenientSentimentThresholdStatus.Suspended || next == LenientSentimentThresholdStatus.Closed;
            case LenientSentimentThresholdStatus.Suspended:
                return next == LenientSentimentThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientSentimentThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient sentiment threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays * FloorValue, 2);
    }

    public LenientSentimentThreshold Copy()
    {
        return new LenientSentimentThreshold(
            Code,
            Label,
            SalaryBand,
            Country,
            GraceDays,
            Amount,
            FloorValue,
            Category,
            DurationMinutes,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

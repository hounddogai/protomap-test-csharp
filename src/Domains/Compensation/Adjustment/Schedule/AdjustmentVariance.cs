namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment variance is in its life cycle.</summary>
public enum AdjustmentVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adjustment variance, with what the planners need to act on it.</summary>
public sealed class AdjustmentVariance(string code, string label, string grade, string currency, string segment, string channel, int durationMinutes, bool archived, string timezone, string department, AdjustmentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public string Department { get; set; } = department;
    public AdjustmentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment variance in status created.</summary>
    public static AdjustmentVariance NewAdjustmentVariance(string code, string label)
    {
        return new AdjustmentVariance(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            false,
            "",
            "",
            AdjustmentVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentVarianceStatus next)
    {
        switch (Status)
        {
            case AdjustmentVarianceStatus.Created:
                return next == AdjustmentVarianceStatus.Validated || next == AdjustmentVarianceStatus.Retired;
            case AdjustmentVarianceStatus.Validated:
                switch (next)
                {
                    case AdjustmentVarianceStatus.Published:
                    case AdjustmentVarianceStatus.Retired:
                        return true;
                }
                return false;
            case AdjustmentVarianceStatus.Published:
                return next == AdjustmentVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdjustmentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public AdjustmentVariance Copy()
    {
        return new AdjustmentVariance(
            Code,
            Label,
            Grade,
            Currency,
            Segment,
            Channel,
            DurationMinutes,
            Archived,
            Timezone,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

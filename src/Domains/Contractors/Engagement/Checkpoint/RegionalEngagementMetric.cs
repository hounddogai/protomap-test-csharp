namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional engagement metric is in its life cycle.</summary>
public enum RegionalEngagementMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One regional engagement metric, with what the planners need to act on it.</summary>
public sealed class RegionalEngagementMetric(string code, string label, int graceDays, int sequence, double ceiling, int limit, double score, string country, int updatedAt, double rate, RegionalEngagementMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public double Ceiling { get; set; } = ceiling;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public RegionalEngagementMetricStatus Status { get; set; } = status;

    /// <summary>Starts a regional engagement metric in status draft.</summary>
    public static RegionalEngagementMetric NewRegionalEngagementMetric(string code, string label)
    {
        return new RegionalEngagementMetric(
            code,
            label,
            0,
            0,
            0.0,
            0,
            0.0,
            "",
            0,
            0.0,
            RegionalEngagementMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalEngagementMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalEngagementMetricStatus next)
    {
        switch (Status)
        {
            case RegionalEngagementMetricStatus.Draft:
                return next == RegionalEngagementMetricStatus.Pending;
            case RegionalEngagementMetricStatus.Pending:
                switch (next)
                {
                    case RegionalEngagementMetricStatus.Closed:
                        return true;
                }
                return false;
            case RegionalEngagementMetricStatus.Active:
                switch (next)
                {
                    case RegionalEngagementMetricStatus.Suspended:
                    case RegionalEngagementMetricStatus.Closed:
                        return true;
                }
                return false;
            case RegionalEngagementMetricStatus.Suspended:
                return next == RegionalEngagementMetricStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalEngagementMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional engagement metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional engagement metric"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Score - (double)UpdatedAt, 2);
    }

    public RegionalEngagementMetric Copy()
    {
        return new RegionalEngagementMetric(
            Code,
            Label,
            GraceDays,
            Sequence,
            Ceiling,
            Limit,
            Score,
            Country,
            UpdatedAt,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

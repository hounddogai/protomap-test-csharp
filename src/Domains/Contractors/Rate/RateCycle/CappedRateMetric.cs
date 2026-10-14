namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped rate metric is in its life cycle.</summary>
public enum CappedRateMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped rate metric, with what the planners need to act on it.</summary>
public sealed class CappedRateMetric(string code, string name, int durationMinutes, int graceDays, Dictionary<string, string> attributes, string unit, double rating, double amount, string notes, string reviewer, int version, CappedRateMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Unit { get; set; } = unit;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public string Notes { get; set; } = notes;
    public string Reviewer { get; set; } = reviewer;
    public int Version { get; set; } = version;
    public CappedRateMetricStatus Status { get; set; } = status;

    /// <summary>Starts a capped rate metric in status idle.</summary>
    public static CappedRateMetric NewCappedRateMetric(string code, string name)
    {
        return new CappedRateMetric(
            code,
            name,
            0,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            CappedRateMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRateMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRateMetricStatus next)
    {
        switch (Status)
        {
            case CappedRateMetricStatus.Idle:
                return next == CappedRateMetricStatus.Ready;
            case CappedRateMetricStatus.Warming:
                return next == CappedRateMetricStatus.Stopped;
            case CappedRateMetricStatus.Ready:
                return next == CappedRateMetricStatus.Stopped;
            case CappedRateMetricStatus.Draining:
                switch (next)
                {
                    case CappedRateMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRateMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped rate metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rating - (double)DurationMinutes, 2);
    }

    public CappedRateMetric Copy()
    {
        return new CappedRateMetric(
            Code,
            Name,
            DurationMinutes,
            GraceDays,
            Attributes,
            Unit,
            Rating,
            Amount,
            Notes,
            Reviewer,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}

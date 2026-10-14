namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh metric is in its life cycle.</summary>
public enum RefreshMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a refresh metric and its current status.</summary>
public sealed class RefreshMetric(string code, string name, double threshold, string locale, string unit, string notes, int durationMinutes, bool active, string category, int batchSize, bool approved, RefreshMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Active { get; set; } = active;
    public string Category { get; set; } = category;
    public int BatchSize { get; set; } = batchSize;
    public bool Approved { get; set; } = approved;
    public RefreshMetricStatus Status { get; set; } = status;

    /// <summary>Starts a refresh metric in status idle.</summary>
    public static RefreshMetric NewRefreshMetric(string code, string name)
    {
        return new RefreshMetric(code, name, 0.0, "", "", "", 0, false, "", 0, false, RefreshMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshMetricStatus next)
    {
        switch (Status)
        {
            case RefreshMetricStatus.Idle:
                switch (next)
                {
                    case RefreshMetricStatus.Warming:
                    case RefreshMetricStatus.Draining:
                        return true;
                }
                return false;
            case RefreshMetricStatus.Warming:
                switch (next)
                {
                    case RefreshMetricStatus.Draining:
                    case RefreshMetricStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshMetricStatus.Ready:
                return next == RefreshMetricStatus.Draining || next == RefreshMetricStatus.Stopped;
            case RefreshMetricStatus.Draining:
                switch (next)
                {
                    case RefreshMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)BatchSize + (double)DurationMinutes, 2);
    }

    public RefreshMetric Copy()
    {
        return new RefreshMetric(
            Code,
            Name,
            Threshold,
            Locale,
            Unit,
            Notes,
            DurationMinutes,
            Active,
            Category,
            BatchSize,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

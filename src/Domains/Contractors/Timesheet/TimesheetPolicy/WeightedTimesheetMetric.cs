namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted timesheet metric is in its life cycle.</summary>
public enum WeightedTimesheetMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One weighted timesheet metric, with what the planners need to act on it.</summary>
public sealed class WeightedTimesheetMetric(string code, string title, string region, int leaveDays, string channel, double margin, string source, double ratio, Dictionary<string, string> attributes, string externalRef, int batchSize, WeightedTimesheetMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public double Ratio { get; set; } = ratio;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public WeightedTimesheetMetricStatus Status { get; set; } = status;

    /// <summary>Starts a weighted timesheet metric in status idle.</summary>
    public static WeightedTimesheetMetric NewWeightedTimesheetMetric(string code, string title)
    {
        return new WeightedTimesheetMetric(
            code,
            title,
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            WeightedTimesheetMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTimesheetMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTimesheetMetricStatus next)
    {
        switch (Status)
        {
            case WeightedTimesheetMetricStatus.Idle:
                switch (next)
                {
                    case WeightedTimesheetMetricStatus.Warming:
                    case WeightedTimesheetMetricStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedTimesheetMetricStatus.Warming:
                switch (next)
                {
                    case WeightedTimesheetMetricStatus.Ready:
                    case WeightedTimesheetMetricStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedTimesheetMetricStatus.Ready:
                return next == WeightedTimesheetMetricStatus.Stopped;
            case WeightedTimesheetMetricStatus.Draining:
                return next == WeightedTimesheetMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WeightedTimesheetMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted timesheet metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)LeaveDays * (double)BatchSize, 2);
    }

    public WeightedTimesheetMetric Copy()
    {
        return new WeightedTimesheetMetric(
            Code,
            Title,
            Region,
            LeaveDays,
            Channel,
            Margin,
            Source,
            Ratio,
            Attributes,
            ExternalRef,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}

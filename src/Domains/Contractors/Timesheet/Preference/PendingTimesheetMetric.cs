namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending timesheet metric is in its life cycle.</summary>
public enum PendingTimesheetMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending timesheet metric, with what the planners need to act on it.</summary>
public sealed class PendingTimesheetMetric(string code, string label, double rating, List<string> tags, string country, int priority, string jobCode, string description, PendingTimesheetMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public int Priority { get; set; } = priority;
    public string JobCode { get; set; } = jobCode;
    public string Description { get; set; } = description;
    public PendingTimesheetMetricStatus Status { get; set; } = status;

    /// <summary>Starts a pending timesheet metric in status requested.</summary>
    public static PendingTimesheetMetric NewPendingTimesheetMetric(string code, string label)
    {
        return new PendingTimesheetMetric(
            code,
            label,
            0.0,
            new List<string>(),
            "",
            0,
            "",
            "",
            PendingTimesheetMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTimesheetMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTimesheetMetricStatus next)
    {
        switch (Status)
        {
            case PendingTimesheetMetricStatus.Requested:
                return next == PendingTimesheetMetricStatus.Granted;
            case PendingTimesheetMetricStatus.Granted:
                switch (next)
                {
                    case PendingTimesheetMetricStatus.Expired:
                        return true;
                }
                return false;
            case PendingTimesheetMetricStatus.Revoked:
                switch (next)
                {
                    case PendingTimesheetMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTimesheetMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending timesheet metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority * Rating, 2);
    }

    public PendingTimesheetMetric Copy()
    {
        return new PendingTimesheetMetric(
            Code,
            Label,
            Rating,
            Tags,
            Country,
            Priority,
            JobCode,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled access badge metric is in its life cycle.</summary>
public enum ScheduledAccessBadgeMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled access badge metric. Its code is unique within a tenant.</summary>
public sealed class ScheduledAccessBadgeMetric(string code, string name, string jobCode, string phone, int limit, ScheduledAccessBadgeMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public ScheduledAccessBadgeMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled access badge metric in status proposed.</summary>
    public static ScheduledAccessBadgeMetric NewScheduledAccessBadgeMetric(string code, string name)
    {
        return new ScheduledAccessBadgeMetric(code, name, "", "", 0, ScheduledAccessBadgeMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccessBadgeMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccessBadgeMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledAccessBadgeMetricStatus.Proposed:
                switch (next)
                {
                    case ScheduledAccessBadgeMetricStatus.Approved:
                    case ScheduledAccessBadgeMetricStatus.Rejected:
                        return true;
                }
                return false;
            case ScheduledAccessBadgeMetricStatus.Approved:
                return next == ScheduledAccessBadgeMetricStatus.Rejected;
            case ScheduledAccessBadgeMetricStatus.Rejected:
                return next == ScheduledAccessBadgeMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledAccessBadgeMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled access badge metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public ScheduledAccessBadgeMetric Copy()
    {
        return new ScheduledAccessBadgeMetric(Code, Name, JobCode, Phone, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

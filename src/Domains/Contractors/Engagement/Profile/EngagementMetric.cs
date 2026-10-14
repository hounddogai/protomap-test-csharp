namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement metric is in its life cycle.</summary>
public enum EngagementMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a engagement metric and its current status.</summary>
public sealed class EngagementMetric(string code, string name, string phone, string location, string reviewer, double minimum, EngagementMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public double Minimum { get; set; } = minimum;
    public EngagementMetricStatus Status { get; set; } = status;

    /// <summary>Starts a engagement metric in status proposed.</summary>
    public static EngagementMetric NewEngagementMetric(string code, string name)
    {
        return new EngagementMetric(code, name, "", "", "", 0.0, EngagementMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementMetricStatus next)
    {
        switch (Status)
        {
            case EngagementMetricStatus.Proposed:
                switch (next)
                {
                    case EngagementMetricStatus.Approved:
                    case EngagementMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case EngagementMetricStatus.Approved:
                switch (next)
                {
                    case EngagementMetricStatus.Rejected:
                    case EngagementMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case EngagementMetricStatus.Rejected:
                return next == EngagementMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(EngagementMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public EngagementMetric Copy()
    {
        return new EngagementMetric(Code, Name, Phone, Location, Reviewer, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

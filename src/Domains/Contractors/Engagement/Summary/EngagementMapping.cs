namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement mapping is in its life cycle.</summary>
public enum EngagementMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a engagement mapping and its current status.</summary>
public sealed class EngagementMapping(string code, string label, double margin, string team, string badge, string salaryBand, EngagementMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Team { get; set; } = team;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public EngagementMappingStatus Status { get; set; } = status;

    /// <summary>Starts a engagement mapping in status idle.</summary>
    public static EngagementMapping NewEngagementMapping(string code, string label)
    {
        return new EngagementMapping(code, label, 0.0, "", "", "", EngagementMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementMappingStatus next)
    {
        switch (Status)
        {
            case EngagementMappingStatus.Idle:
                switch (next)
                {
                    case EngagementMappingStatus.Warming:
                    case EngagementMappingStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementMappingStatus.Warming:
                switch (next)
                {
                    case EngagementMappingStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementMappingStatus.Ready:
                switch (next)
                {
                    case EngagementMappingStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementMappingStatus.Draining:
                return next == EngagementMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EngagementMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public EngagementMapping Copy()
    {
        return new EngagementMapping(Code, Label, Margin, Team, Badge, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

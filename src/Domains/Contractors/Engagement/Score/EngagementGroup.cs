namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement group is in its life cycle.</summary>
public enum EngagementGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a engagement group and its current status.</summary>
public sealed class EngagementGroup(string code, string label, int durationMinutes, string employeeId, bool active, double hours, string location, int graceDays, EngagementGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public double Hours { get; set; } = hours;
    public string Location { get; set; } = location;
    public int GraceDays { get; set; } = graceDays;
    public EngagementGroupStatus Status { get; set; } = status;

    /// <summary>Starts a engagement group in status new.</summary>
    public static EngagementGroup NewEngagementGroup(string code, string label)
    {
        return new EngagementGroup(code, label, 0, "", false, 0.0, "", 0, EngagementGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementGroupStatus next)
    {
        switch (Status)
        {
            case EngagementGroupStatus.New:
                switch (next)
                {
                    case EngagementGroupStatus.Queued:
                        return true;
                }
                return false;
            case EngagementGroupStatus.Queued:
                return next == EngagementGroupStatus.Running;
            case EngagementGroupStatus.Running:
                return next == EngagementGroupStatus.Failed;
            case EngagementGroupStatus.Done:
                return next == EngagementGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(EngagementGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"engagement group"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays * Hours, 2);
    }

    public EngagementGroup Copy()
    {
        return new EngagementGroup(
            Code,
            Label,
            DurationMinutes,
            EmployeeId,
            Active,
            Hours,
            Location,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

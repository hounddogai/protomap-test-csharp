namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement milestone is in its life cycle.</summary>
public enum EngagementMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a engagement milestone and its current status.</summary>
public sealed class EngagementMilestone(string code, string label, double hours, int durationMinutes, double rate, EngagementMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public EngagementMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a engagement milestone in status proposed.</summary>
    public static EngagementMilestone NewEngagementMilestone(string code, string label)
    {
        return new EngagementMilestone(code, label, 0.0, 0, 0.0, EngagementMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementMilestoneStatus next)
    {
        switch (Status)
        {
            case EngagementMilestoneStatus.Proposed:
                switch (next)
                {
                    case EngagementMilestoneStatus.Rejected:
                        return true;
                }
                return false;
            case EngagementMilestoneStatus.Approved:
                return next == EngagementMilestoneStatus.Rejected;
            case EngagementMilestoneStatus.Rejected:
                switch (next)
                {
                    case EngagementMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours + (double)DurationMinutes, 2);
    }

    public EngagementMilestone Copy()
    {
        return new EngagementMilestone(Code, Label, Hours, DurationMinutes, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending feedback milestone is in its life cycle.</summary>
public enum PendingFeedbackMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pending feedback milestone, with what the planners need to act on it.</summary>
public sealed class PendingFeedbackMilestone(string code, string name, int graceDays, double score, string employeeId, double hours, int attempts, bool archived, string location, int leaveDays, List<string> tags, PendingFeedbackMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public double Hours { get; set; } = hours;
    public int Attempts { get; set; } = attempts;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public PendingFeedbackMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pending feedback milestone in status proposed.</summary>
    public static PendingFeedbackMilestone NewPendingFeedbackMilestone(string code, string name)
    {
        return new PendingFeedbackMilestone(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            0,
            false,
            "",
            0,
            new List<string>(),
            PendingFeedbackMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingFeedbackMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingFeedbackMilestoneStatus next)
    {
        switch (Status)
        {
            case PendingFeedbackMilestoneStatus.Proposed:
                switch (next)
                {
                    case PendingFeedbackMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingFeedbackMilestoneStatus.Approved:
                switch (next)
                {
                    case PendingFeedbackMilestoneStatus.Rejected:
                    case PendingFeedbackMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingFeedbackMilestoneStatus.Rejected:
                switch (next)
                {
                    case PendingFeedbackMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingFeedbackMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending feedback milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double AdjustedValue()
    {
        return Math.Round(Score * Hours, 2);
    }

    public PendingFeedbackMilestone Copy()
    {
        return new PendingFeedbackMilestone(
            Code,
            Name,
            GraceDays,
            Score,
            EmployeeId,
            Hours,
            Attempts,
            Archived,
            Location,
            LeaveDays,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

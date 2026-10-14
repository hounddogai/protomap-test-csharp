namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request milestone is in its life cycle.</summary>
public enum LeaveRequestMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One leave request milestone, with what the planners need to act on it.</summary>
public sealed class LeaveRequestMilestone(string code, string title, double cost, int sequence, double minimum, LeaveRequestMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public double Minimum { get; set; } = minimum;
    public LeaveRequestMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a leave request milestone in status planned.</summary>
    public static LeaveRequestMilestone NewLeaveRequestMilestone(string code, string title)
    {
        return new LeaveRequestMilestone(code, title, 0.0, 0, 0.0, LeaveRequestMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestMilestoneStatus next)
    {
        switch (Status)
        {
            case LeaveRequestMilestoneStatus.Planned:
                return next == LeaveRequestMilestoneStatus.Completed || next == LeaveRequestMilestoneStatus.Cancelled;
            case LeaveRequestMilestoneStatus.Scheduled:
                return next == LeaveRequestMilestoneStatus.InProgress;
            case LeaveRequestMilestoneStatus.InProgress:
                switch (next)
                {
                    case LeaveRequestMilestoneStatus.Completed:
                    case LeaveRequestMilestoneStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestMilestoneStatus.Completed:
                return next == LeaveRequestMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"leave request milestone"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Sequence + Cost, 2);
    }

    public LeaveRequestMilestone Copy()
    {
        return new LeaveRequestMilestone(Code, Title, Cost, Sequence, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request override is in its life cycle.</summary>
public enum LeaveRequestOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One leave request override, with what the planners need to act on it.</summary>
public sealed class LeaveRequestOverride(string code, string label, int graceDays, string channel, int priority, string grade, string checksum, string reviewer, LeaveRequestOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public int Priority { get; set; } = priority;
    public string Grade { get; set; } = grade;
    public string Checksum { get; set; } = checksum;
    public string Reviewer { get; set; } = reviewer;
    public LeaveRequestOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a leave request override in status open.</summary>
    public static LeaveRequestOverride NewLeaveRequestOverride(string code, string label)
    {
        return new LeaveRequestOverride(code, label, 0, "", 0, "", "", "", LeaveRequestOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestOverrideStatus next)
    {
        switch (Status)
        {
            case LeaveRequestOverrideStatus.Open:
                switch (next)
                {
                    case LeaveRequestOverrideStatus.Resolved:
                    case LeaveRequestOverrideStatus.Archived:
                        return true;
                }
                return false;
            case LeaveRequestOverrideStatus.InReview:
                switch (next)
                {
                    case LeaveRequestOverrideStatus.Resolved:
                        return true;
                }
                return false;
            case LeaveRequestOverrideStatus.Resolved:
                switch (next)
                {
                    case LeaveRequestOverrideStatus.Reopened:
                        return true;
                }
                return false;
            case LeaveRequestOverrideStatus.Reopened:
                switch (next)
                {
                    case LeaveRequestOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)GraceDays - (double)Priority, 2);
    }

    public LeaveRequestOverride Copy()
    {
        return new LeaveRequestOverride(
            Code,
            Label,
            GraceDays,
            Channel,
            Priority,
            Grade,
            Checksum,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

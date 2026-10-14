namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request entry is in its life cycle.</summary>
public enum LeaveRequestEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A leave request entry as the leave request team keeps it.</summary>
public sealed class LeaveRequestEntry(string code, string name, bool visible, double floorValue, string summary, int priority, string reviewer, double rating, string label, LeaveRequestEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public double FloorValue { get; set; } = floorValue;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public LeaveRequestEntryStatus Status { get; set; } = status;

    /// <summary>Starts a leave request entry in status idle.</summary>
    public static LeaveRequestEntry NewLeaveRequestEntry(string code, string name)
    {
        return new LeaveRequestEntry(code, name, false, 0.0, "", 0, "", 0.0, "", LeaveRequestEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestEntryStatus next)
    {
        switch (Status)
        {
            case LeaveRequestEntryStatus.Idle:
                switch (next)
                {
                    case LeaveRequestEntryStatus.Warming:
                        return true;
                }
                return false;
            case LeaveRequestEntryStatus.Warming:
                return next == LeaveRequestEntryStatus.Draining;
            case LeaveRequestEntryStatus.Ready:
                switch (next)
                {
                    case LeaveRequestEntryStatus.Draining:
                    case LeaveRequestEntryStatus.Stopped:
                        return true;
                }
                return false;
            case LeaveRequestEntryStatus.Draining:
                return next == LeaveRequestEntryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LeaveRequestEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating + FloorValue, 2);
    }

    public LeaveRequestEntry Copy()
    {
        return new LeaveRequestEntry(
            Code,
            Name,
            Visible,
            FloorValue,
            Summary,
            Priority,
            Reviewer,
            Rating,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

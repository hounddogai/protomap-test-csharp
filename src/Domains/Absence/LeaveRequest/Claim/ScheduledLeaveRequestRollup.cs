namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request rollup is in its life cycle.</summary>
public enum ScheduledLeaveRequestRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled leave request rollup, with what the planners need to act on it.</summary>
public sealed class ScheduledLeaveRequestRollup(string code, string label, string channel, string checksum, int expiresAt, ScheduledLeaveRequestRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledLeaveRequestRollupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request rollup in status planned.</summary>
    public static ScheduledLeaveRequestRollup NewScheduledLeaveRequestRollup(string code, string label)
    {
        return new ScheduledLeaveRequestRollup(code, label, "", "", 0, ScheduledLeaveRequestRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestRollupStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestRollupStatus.Planned:
                switch (next)
                {
                    case ScheduledLeaveRequestRollupStatus.Scheduled:
                    case ScheduledLeaveRequestRollupStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestRollupStatus.Scheduled:
                return next == ScheduledLeaveRequestRollupStatus.Completed;
            case ScheduledLeaveRequestRollupStatus.InProgress:
                switch (next)
                {
                    case ScheduledLeaveRequestRollupStatus.Completed:
                    case ScheduledLeaveRequestRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestRollupStatus.Completed:
                switch (next)
                {
                    case ScheduledLeaveRequestRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public ScheduledLeaveRequestRollup Copy()
    {
        return new ScheduledLeaveRequestRollup(Code, Label, Channel, Checksum, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

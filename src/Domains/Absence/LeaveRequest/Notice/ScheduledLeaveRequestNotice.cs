namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request notice is in its life cycle.</summary>
public enum ScheduledLeaveRequestNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One scheduled leave request notice, with what the planners need to act on it.</summary>
public sealed class ScheduledLeaveRequestNotice(string code, string name, string channel, bool pinned, int expiresAt, ScheduledLeaveRequestNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledLeaveRequestNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request notice in status draft.</summary>
    public static ScheduledLeaveRequestNotice NewScheduledLeaveRequestNotice(string code, string name)
    {
        return new ScheduledLeaveRequestNotice(code, name, "", false, 0, ScheduledLeaveRequestNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestNoticeStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestNoticeStatus.Draft:
                switch (next)
                {
                    case ScheduledLeaveRequestNoticeStatus.Pending:
                    case ScheduledLeaveRequestNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestNoticeStatus.Pending:
                switch (next)
                {
                    case ScheduledLeaveRequestNoticeStatus.Active:
                    case ScheduledLeaveRequestNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestNoticeStatus.Active:
                return next == ScheduledLeaveRequestNoticeStatus.Suspended || next == ScheduledLeaveRequestNoticeStatus.Closed;
            case ScheduledLeaveRequestNoticeStatus.Suspended:
                return next == ScheduledLeaveRequestNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public ScheduledLeaveRequestNotice Copy()
    {
        return new ScheduledLeaveRequestNotice(Code, Name, Channel, Pinned, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

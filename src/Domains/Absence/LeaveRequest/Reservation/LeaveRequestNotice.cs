namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request notice is in its life cycle.</summary>
public enum LeaveRequestNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a leave request notice and its current status.</summary>
public sealed class LeaveRequestNotice(string code, string name, double weight, int leaveDays, int attempts, string description, string checksum, LeaveRequestNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public LeaveRequestNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a leave request notice in status proposed.</summary>
    public static LeaveRequestNotice NewLeaveRequestNotice(string code, string name)
    {
        return new LeaveRequestNotice(code, name, 0.0, 0, 0, "", "", LeaveRequestNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestNoticeStatus next)
    {
        switch (Status)
        {
            case LeaveRequestNoticeStatus.Proposed:
                switch (next)
                {
                    case LeaveRequestNoticeStatus.Approved:
                    case LeaveRequestNoticeStatus.Rejected:
                        return true;
                }
                return false;
            case LeaveRequestNoticeStatus.Approved:
                switch (next)
                {
                    case LeaveRequestNoticeStatus.Rejected:
                    case LeaveRequestNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case LeaveRequestNoticeStatus.Rejected:
                return next == LeaveRequestNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LeaveRequestNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Attempts + Weight, 2);
    }

    public LeaveRequestNotice Copy()
    {
        return new LeaveRequestNotice(Code, Name, Weight, LeaveDays, Attempts, Description, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

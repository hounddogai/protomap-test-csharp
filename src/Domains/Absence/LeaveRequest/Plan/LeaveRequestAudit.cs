namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request audit is in its life cycle.</summary>
public enum LeaveRequestAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A leave request audit. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestAudit(string code, string title, string team, string checksum, double fte, LeaveRequestAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public LeaveRequestAuditStatus Status { get; set; } = status;

    /// <summary>Starts a leave request audit in status planned.</summary>
    public static LeaveRequestAudit NewLeaveRequestAudit(string code, string title)
    {
        return new LeaveRequestAudit(code, title, "", "", 0.0, LeaveRequestAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestAuditStatus next)
    {
        switch (Status)
        {
            case LeaveRequestAuditStatus.Planned:
                return next == LeaveRequestAuditStatus.Scheduled || next == LeaveRequestAuditStatus.Cancelled;
            case LeaveRequestAuditStatus.Scheduled:
                return next == LeaveRequestAuditStatus.InProgress || next == LeaveRequestAuditStatus.Completed;
            case LeaveRequestAuditStatus.InProgress:
                switch (next)
                {
                    case LeaveRequestAuditStatus.Completed:
                        return true;
                }
                return false;
            case LeaveRequestAuditStatus.Completed:
                return next == LeaveRequestAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public LeaveRequestAudit Copy()
    {
        return new LeaveRequestAudit(Code, Title, Team, Checksum, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict booking audit is in its life cycle.</summary>
public enum StrictBookingAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One strict booking audit, with what the planners need to act on it.</summary>
public sealed class StrictBookingAudit(string code, string label, double fte, string managerId, double amount, string team, double score, string summary, string employeeId, StrictBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public StrictBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a strict booking audit in status planned.</summary>
    public static StrictBookingAudit NewStrictBookingAudit(string code, string label)
    {
        return new StrictBookingAudit(code, label, 0.0, "", 0.0, "", 0.0, "", "", StrictBookingAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBookingAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBookingAuditStatus next)
    {
        switch (Status)
        {
            case StrictBookingAuditStatus.Planned:
                switch (next)
                {
                    case StrictBookingAuditStatus.Scheduled:
                    case StrictBookingAuditStatus.InProgress:
                        return true;
                }
                return false;
            case StrictBookingAuditStatus.Scheduled:
                return next == StrictBookingAuditStatus.Cancelled;
            case StrictBookingAuditStatus.InProgress:
                switch (next)
                {
                    case StrictBookingAuditStatus.Completed:
                        return true;
                }
                return false;
            case StrictBookingAuditStatus.Completed:
                return next == StrictBookingAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Fte * Score, 2);
    }

    public StrictBookingAudit Copy()
    {
        return new StrictBookingAudit(Code, Label, Fte, ManagerId, Amount, Team, Score, Summary, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

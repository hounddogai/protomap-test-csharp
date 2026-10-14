namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request score is in its life cycle.</summary>
public enum LeaveRequestScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A leave request score. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestScore(string code, string title, string employeeId, string source, double cost, LeaveRequestScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public LeaveRequestScoreStatus Status { get; set; } = status;

    /// <summary>Starts a leave request score in status planned.</summary>
    public static LeaveRequestScore NewLeaveRequestScore(string code, string title)
    {
        return new LeaveRequestScore(code, title, "", "", 0.0, LeaveRequestScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestScoreStatus next)
    {
        switch (Status)
        {
            case LeaveRequestScoreStatus.Planned:
                return next == LeaveRequestScoreStatus.Scheduled || next == LeaveRequestScoreStatus.Cancelled;
            case LeaveRequestScoreStatus.Scheduled:
                return next == LeaveRequestScoreStatus.InProgress || next == LeaveRequestScoreStatus.Cancelled;
            case LeaveRequestScoreStatus.InProgress:
                switch (next)
                {
                    case LeaveRequestScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestScoreStatus.Completed:
                switch (next)
                {
                    case LeaveRequestScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public LeaveRequestScore Copy()
    {
        return new LeaveRequestScore(Code, Title, EmployeeId, Source, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}

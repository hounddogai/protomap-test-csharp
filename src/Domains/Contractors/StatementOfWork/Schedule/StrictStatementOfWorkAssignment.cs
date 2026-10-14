namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict statement of work assignment is in its life cycle.</summary>
public enum StrictStatementOfWorkAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One strict statement of work assignment, with what the planners need to act on it.</summary>
public sealed class StrictStatementOfWorkAssignment(string code, string name, double margin, string segment, int startsAt, string salaryBand, StrictStatementOfWorkAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string Segment { get; set; } = segment;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public StrictStatementOfWorkAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict statement of work assignment in status planned.</summary>
    public static StrictStatementOfWorkAssignment NewStrictStatementOfWorkAssignment(string code, string name)
    {
        return new StrictStatementOfWorkAssignment(
            code,
            name,
            0.0,
            "",
            0,
            "",
            StrictStatementOfWorkAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictStatementOfWorkAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictStatementOfWorkAssignmentStatus next)
    {
        switch (Status)
        {
            case StrictStatementOfWorkAssignmentStatus.Planned:
                return next == StrictStatementOfWorkAssignmentStatus.Scheduled || next == StrictStatementOfWorkAssignmentStatus.Cancelled;
            case StrictStatementOfWorkAssignmentStatus.Scheduled:
                switch (next)
                {
                    case StrictStatementOfWorkAssignmentStatus.InProgress:
                        return true;
                }
                return false;
            case StrictStatementOfWorkAssignmentStatus.InProgress:
                return next == StrictStatementOfWorkAssignmentStatus.Completed;
            case StrictStatementOfWorkAssignmentStatus.Completed:
                switch (next)
                {
                    case StrictStatementOfWorkAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictStatementOfWorkAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict statement of work assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict statement of work assignment"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)StartsAt - Margin, 2);
    }

    public StrictStatementOfWorkAssignment Copy()
    {
        return new StrictStatementOfWorkAssignment(Code, Name, Margin, Segment, StartsAt, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

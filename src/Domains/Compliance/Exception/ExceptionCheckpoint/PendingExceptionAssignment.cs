namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception assignment is in its life cycle.</summary>
public enum PendingExceptionAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending exception assignment, with what the planners need to act on it.</summary>
public sealed class PendingExceptionAssignment(string code, string title, int graceDays, bool approved, string reviewer, string notes, PendingExceptionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public PendingExceptionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception assignment in status new.</summary>
    public static PendingExceptionAssignment NewPendingExceptionAssignment(string code, string title)
    {
        return new PendingExceptionAssignment(code, title, 0, false, "", "", PendingExceptionAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionAssignmentStatus next)
    {
        switch (Status)
        {
            case PendingExceptionAssignmentStatus.New:
                switch (next)
                {
                    case PendingExceptionAssignmentStatus.Queued:
                    case PendingExceptionAssignmentStatus.Running:
                        return true;
                }
                return false;
            case PendingExceptionAssignmentStatus.Queued:
                switch (next)
                {
                    case PendingExceptionAssignmentStatus.Running:
                    case PendingExceptionAssignmentStatus.Done:
                        return true;
                }
                return false;
            case PendingExceptionAssignmentStatus.Running:
                switch (next)
                {
                    case PendingExceptionAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case PendingExceptionAssignmentStatus.Done:
                return next == PendingExceptionAssignmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingExceptionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public PendingExceptionAssignment Copy()
    {
        return new PendingExceptionAssignment(Code, Title, GraceDays, Approved, Reviewer, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

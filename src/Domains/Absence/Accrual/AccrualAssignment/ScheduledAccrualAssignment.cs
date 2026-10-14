namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled accrual assignment is in its life cycle.</summary>
public enum ScheduledAccrualAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled accrual assignment as the accrual team keeps it.</summary>
public sealed class ScheduledAccrualAssignment(string code, string label, double budget, string summary, string externalRef, ScheduledAccrualAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public string ExternalRef { get; set; } = externalRef;
    public ScheduledAccrualAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled accrual assignment in status draft.</summary>
    public static ScheduledAccrualAssignment NewScheduledAccrualAssignment(string code, string label)
    {
        return new ScheduledAccrualAssignment(code, label, 0.0, "", "", ScheduledAccrualAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccrualAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccrualAssignmentStatus next)
    {
        switch (Status)
        {
            case ScheduledAccrualAssignmentStatus.Draft:
                switch (next)
                {
                    case ScheduledAccrualAssignmentStatus.Pending:
                    case ScheduledAccrualAssignmentStatus.Active:
                        return true;
                }
                return false;
            case ScheduledAccrualAssignmentStatus.Pending:
                switch (next)
                {
                    case ScheduledAccrualAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledAccrualAssignmentStatus.Active:
                return next == ScheduledAccrualAssignmentStatus.Suspended || next == ScheduledAccrualAssignmentStatus.Closed;
            case ScheduledAccrualAssignmentStatus.Suspended:
                return next == ScheduledAccrualAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledAccrualAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled accrual assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public ScheduledAccrualAssignment Copy()
    {
        return new ScheduledAccrualAssignment(Code, Label, Budget, Summary, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

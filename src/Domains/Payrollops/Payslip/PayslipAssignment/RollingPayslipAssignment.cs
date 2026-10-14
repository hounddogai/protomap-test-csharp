namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling payslip assignment is in its life cycle.</summary>
public enum RollingPayslipAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling payslip assignment and its current status.</summary>
public sealed class RollingPayslipAssignment(string code, string label, string team, string currency, int capacity, RollingPayslipAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public int Capacity { get; set; } = capacity;
    public RollingPayslipAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling payslip assignment in status open.</summary>
    public static RollingPayslipAssignment NewRollingPayslipAssignment(string code, string label)
    {
        return new RollingPayslipAssignment(code, label, "", "", 0, RollingPayslipAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayslipAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayslipAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingPayslipAssignmentStatus.Open:
                switch (next)
                {
                    case RollingPayslipAssignmentStatus.Resolved:
                    case RollingPayslipAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case RollingPayslipAssignmentStatus.InReview:
                return next == RollingPayslipAssignmentStatus.Resolved;
            case RollingPayslipAssignmentStatus.Resolved:
                return next == RollingPayslipAssignmentStatus.Reopened || next == RollingPayslipAssignmentStatus.Archived;
            case RollingPayslipAssignmentStatus.Reopened:
                return next == RollingPayslipAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingPayslipAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling payslip assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling payslip assignment"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public RollingPayslipAssignment Copy()
    {
        return new RollingPayslipAssignment(Code, Label, Team, Currency, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip assignment is in its life cycle.</summary>
public enum PayslipAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One payslip assignment, with what the planners need to act on it.</summary>
public sealed class PayslipAssignment(string code, string title, string employeeId, string ownerId, string salaryBand, string summary, double amount, PayslipAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public PayslipAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a payslip assignment in status idle.</summary>
    public static PayslipAssignment NewPayslipAssignment(string code, string title)
    {
        return new PayslipAssignment(code, title, "", "", "", "", 0.0, PayslipAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipAssignmentStatus next)
    {
        switch (Status)
        {
            case PayslipAssignmentStatus.Idle:
                switch (next)
                {
                    case PayslipAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case PayslipAssignmentStatus.Warming:
                switch (next)
                {
                    case PayslipAssignmentStatus.Ready:
                        return true;
                }
                return false;
            case PayslipAssignmentStatus.Ready:
                switch (next)
                {
                    case PayslipAssignmentStatus.Draining:
                    case PayslipAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case PayslipAssignmentStatus.Draining:
                switch (next)
                {
                    case PayslipAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip assignment"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PayslipAssignment Copy()
    {
        return new PayslipAssignment(Code, Title, EmployeeId, OwnerId, SalaryBand, Summary, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled payslip allocation is in its life cycle.</summary>
public enum ScheduledPayslipAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled payslip allocation. Its code is unique within a tenant.</summary>
public sealed class ScheduledPayslipAllocation(string code, string label, int leaveDays, double fte, string title, string grade, ScheduledPayslipAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Fte { get; set; } = fte;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public ScheduledPayslipAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled payslip allocation in status requested.</summary>
    public static ScheduledPayslipAllocation NewScheduledPayslipAllocation(string code, string label)
    {
        return new ScheduledPayslipAllocation(
            code,
            label,
            0,
            0.0,
            "",
            "",
            ScheduledPayslipAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayslipAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayslipAllocationStatus next)
    {
        switch (Status)
        {
            case ScheduledPayslipAllocationStatus.Requested:
                switch (next)
                {
                    case ScheduledPayslipAllocationStatus.Revoked:
                    case ScheduledPayslipAllocationStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledPayslipAllocationStatus.Granted:
                switch (next)
                {
                    case ScheduledPayslipAllocationStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledPayslipAllocationStatus.Revoked:
                return next == ScheduledPayslipAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledPayslipAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled payslip allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte - (double)LeaveDays, 2);
    }

    public ScheduledPayslipAllocation Copy()
    {
        return new ScheduledPayslipAllocation(Code, Label, LeaveDays, Fte, Title, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

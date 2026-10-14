namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip allocation is in its life cycle.</summary>
public enum PayslipAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a payslip allocation and its current status.</summary>
public sealed class PayslipAllocation(string code, string title, bool active, double threshold, string badge, PayslipAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public PayslipAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a payslip allocation in status new.</summary>
    public static PayslipAllocation NewPayslipAllocation(string code, string title)
    {
        return new PayslipAllocation(code, title, false, 0.0, "", PayslipAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipAllocationStatus next)
    {
        switch (Status)
        {
            case PayslipAllocationStatus.New:
                switch (next)
                {
                    case PayslipAllocationStatus.Done:
                        return true;
                }
                return false;
            case PayslipAllocationStatus.Queued:
                return next == PayslipAllocationStatus.Running;
            case PayslipAllocationStatus.Running:
                switch (next)
                {
                    case PayslipAllocationStatus.Done:
                    case PayslipAllocationStatus.Failed:
                        return true;
                }
                return false;
            case PayslipAllocationStatus.Done:
                switch (next)
                {
                    case PayslipAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public PayslipAllocation Copy()
    {
        return new PayslipAllocation(Code, Title, Active, Threshold, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

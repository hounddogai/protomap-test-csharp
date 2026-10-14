namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip rule is in its life cycle.</summary>
public enum PayslipRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a payslip rule and its current status.</summary>
public sealed class PayslipRule(string code, string label, double hours, double amount, int attempts, int capacity, PayslipRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public PayslipRuleStatus Status { get; set; } = status;

    /// <summary>Starts a payslip rule in status new.</summary>
    public static PayslipRule NewPayslipRule(string code, string label)
    {
        return new PayslipRule(code, label, 0.0, 0.0, 0, 0, PayslipRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipRuleStatus next)
    {
        switch (Status)
        {
            case PayslipRuleStatus.New:
                return next == PayslipRuleStatus.Queued || next == PayslipRuleStatus.Failed;
            case PayslipRuleStatus.Queued:
                return next == PayslipRuleStatus.Running || next == PayslipRuleStatus.Failed;
            case PayslipRuleStatus.Running:
                return next == PayslipRuleStatus.Failed;
            case PayslipRuleStatus.Done:
                return next == PayslipRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PayslipRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount + Hours, 2);
    }

    public PayslipRule Copy()
    {
        return new PayslipRule(Code, Label, Hours, Amount, Attempts, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

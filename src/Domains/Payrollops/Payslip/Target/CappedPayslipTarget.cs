namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped payslip target is in its life cycle.</summary>
public enum CappedPayslipTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a capped payslip target and its current status.</summary>
public sealed class CappedPayslipTarget(string code, string label, double margin, int sequence, bool active, bool visible, string title, string reviewer, int windowDays, CappedPayslipTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public bool Active { get; set; } = active;
    public bool Visible { get; set; } = visible;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int WindowDays { get; set; } = windowDays;
    public CappedPayslipTargetStatus Status { get; set; } = status;

    /// <summary>Starts a capped payslip target in status planned.</summary>
    public static CappedPayslipTarget NewCappedPayslipTarget(string code, string label)
    {
        return new CappedPayslipTarget(
            code,
            label,
            0.0,
            0,
            false,
            false,
            "",
            "",
            0,
            CappedPayslipTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayslipTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayslipTargetStatus next)
    {
        switch (Status)
        {
            case CappedPayslipTargetStatus.Planned:
                switch (next)
                {
                    case CappedPayslipTargetStatus.Scheduled:
                        return true;
                }
                return false;
            case CappedPayslipTargetStatus.Scheduled:
                switch (next)
                {
                    case CappedPayslipTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedPayslipTargetStatus.InProgress:
                return next == CappedPayslipTargetStatus.Completed || next == CappedPayslipTargetStatus.Cancelled;
            case CappedPayslipTargetStatus.Completed:
                return next == CappedPayslipTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedPayslipTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped payslip target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Sequence * Margin, 2);
    }

    public CappedPayslipTarget Copy()
    {
        return new CappedPayslipTarget(
            Code,
            Label,
            Margin,
            Sequence,
            Active,
            Visible,
            Title,
            Reviewer,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

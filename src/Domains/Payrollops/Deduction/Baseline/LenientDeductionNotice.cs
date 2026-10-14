namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction notice is in its life cycle.</summary>
public enum LenientDeductionNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient deduction notice. Its code is unique within a tenant.</summary>
public sealed class LenientDeductionNotice(string code, string label, string grade, string costCenter, string salaryBand, int tenureMonths, LenientDeductionNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string CostCenter { get; set; } = costCenter;
    public string SalaryBand { get; set; } = salaryBand;
    public int TenureMonths { get; set; } = tenureMonths;
    public LenientDeductionNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction notice in status idle.</summary>
    public static LenientDeductionNotice NewLenientDeductionNotice(string code, string label)
    {
        return new LenientDeductionNotice(code, label, "", "", "", 0, LenientDeductionNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionNoticeStatus next)
    {
        switch (Status)
        {
            case LenientDeductionNoticeStatus.Idle:
                return next == LenientDeductionNoticeStatus.Ready;
            case LenientDeductionNoticeStatus.Warming:
                switch (next)
                {
                    case LenientDeductionNoticeStatus.Draining:
                    case LenientDeductionNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case LenientDeductionNoticeStatus.Ready:
                switch (next)
                {
                    case LenientDeductionNoticeStatus.Draining:
                    case LenientDeductionNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case LenientDeductionNoticeStatus.Draining:
                return next == LenientDeductionNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientDeductionNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient deduction notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public LenientDeductionNotice Copy()
    {
        return new LenientDeductionNotice(Code, Label, Grade, CostCenter, SalaryBand, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

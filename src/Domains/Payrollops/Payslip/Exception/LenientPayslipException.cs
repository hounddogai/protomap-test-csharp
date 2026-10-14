namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient payslip exception is in its life cycle.</summary>
public enum LenientPayslipExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient payslip exception. Its code is unique within a tenant.</summary>
public sealed class LenientPayslipException(string code, string label, string name, double discount, string segment, string managerId, double floorValue, double fte, LenientPayslipExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public string ManagerId { get; set; } = managerId;
    public double FloorValue { get; set; } = floorValue;
    public double Fte { get; set; } = fte;
    public LenientPayslipExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient payslip exception in status idle.</summary>
    public static LenientPayslipException NewLenientPayslipException(string code, string label)
    {
        return new LenientPayslipException(
            code,
            label,
            "",
            0.0,
            "",
            "",
            0.0,
            0.0,
            LenientPayslipExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayslipExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayslipExceptionStatus next)
    {
        switch (Status)
        {
            case LenientPayslipExceptionStatus.Idle:
                switch (next)
                {
                    case LenientPayslipExceptionStatus.Warming:
                    case LenientPayslipExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case LenientPayslipExceptionStatus.Warming:
                switch (next)
                {
                    case LenientPayslipExceptionStatus.Ready:
                    case LenientPayslipExceptionStatus.Draining:
                        return true;
                }
                return false;
            case LenientPayslipExceptionStatus.Ready:
                return next == LenientPayslipExceptionStatus.Stopped;
            case LenientPayslipExceptionStatus.Draining:
                return next == LenientPayslipExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientPayslipExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient payslip exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(FloorValue + Discount, 2);
    }

    public LenientPayslipException Copy()
    {
        return new LenientPayslipException(Code, Label, Name, Discount, Segment, ManagerId, FloorValue, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

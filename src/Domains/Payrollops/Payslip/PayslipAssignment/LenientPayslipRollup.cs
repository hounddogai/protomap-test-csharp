namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient payslip rollup is in its life cycle.</summary>
public enum LenientPayslipRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient payslip rollup. Its code is unique within a tenant.</summary>
public sealed class LenientPayslipRollup(string code, string title, double threshold, string name, double minimum, int leaveDays, string externalRef, string jobCode, LenientPayslipRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public LenientPayslipRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient payslip rollup in status created.</summary>
    public static LenientPayslipRollup NewLenientPayslipRollup(string code, string title)
    {
        return new LenientPayslipRollup(code, title, 0.0, "", 0.0, 0, "", "", LenientPayslipRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayslipRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayslipRollupStatus next)
    {
        switch (Status)
        {
            case LenientPayslipRollupStatus.Created:
                return next == LenientPayslipRollupStatus.Retired;
            case LenientPayslipRollupStatus.Validated:
                switch (next)
                {
                    case LenientPayslipRollupStatus.Published:
                    case LenientPayslipRollupStatus.Retired:
                        return true;
                }
                return false;
            case LenientPayslipRollupStatus.Published:
                return next == LenientPayslipRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientPayslipRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient payslip rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Threshold - Minimum, 2);
    }

    public LenientPayslipRollup Copy()
    {
        return new LenientPayslipRollup(
            Code,
            Title,
            Threshold,
            Name,
            Minimum,
            LeaveDays,
            ExternalRef,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

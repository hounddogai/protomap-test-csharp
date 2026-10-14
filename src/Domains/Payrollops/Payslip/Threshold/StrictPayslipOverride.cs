namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict payslip override is in its life cycle.</summary>
public enum StrictPayslipOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict payslip override. Its code is unique within a tenant.</summary>
public sealed class StrictPayslipOverride(string code, string title, string timezone, string unit, bool visible, int limit, StrictPayslipOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public StrictPayslipOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict payslip override in status open.</summary>
    public static StrictPayslipOverride NewStrictPayslipOverride(string code, string title)
    {
        return new StrictPayslipOverride(code, title, "", "", false, 0, StrictPayslipOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPayslipOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPayslipOverrideStatus next)
    {
        switch (Status)
        {
            case StrictPayslipOverrideStatus.Open:
                return next == StrictPayslipOverrideStatus.Reopened;
            case StrictPayslipOverrideStatus.InReview:
                return next == StrictPayslipOverrideStatus.Reopened;
            case StrictPayslipOverrideStatus.Resolved:
                switch (next)
                {
                    case StrictPayslipOverrideStatus.Reopened:
                        return true;
                }
                return false;
            case StrictPayslipOverrideStatus.Reopened:
                return next == StrictPayslipOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictPayslipOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict payslip override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public StrictPayslipOverride Copy()
    {
        return new StrictPayslipOverride(Code, Title, Timezone, Unit, Visible, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}

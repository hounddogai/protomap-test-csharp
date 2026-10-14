namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip override is in its life cycle.</summary>
public enum PayslipOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a payslip override and its current status.</summary>
public sealed class PayslipOverride(string code, string label, string location, double discount, int leaveDays, PayslipOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public double Discount { get; set; } = discount;
    public int LeaveDays { get; set; } = leaveDays;
    public PayslipOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a payslip override in status created.</summary>
    public static PayslipOverride NewPayslipOverride(string code, string label)
    {
        return new PayslipOverride(code, label, "", 0.0, 0, PayslipOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipOverrideStatus next)
    {
        switch (Status)
        {
            case PayslipOverrideStatus.Created:
                return next == PayslipOverrideStatus.Validated;
            case PayslipOverrideStatus.Validated:
                switch (next)
                {
                    case PayslipOverrideStatus.Published:
                    case PayslipOverrideStatus.Retired:
                        return true;
                }
                return false;
            case PayslipOverrideStatus.Published:
                switch (next)
                {
                    case PayslipOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round(Discount + (double)LeaveDays, 2);
    }

    public PayslipOverride Copy()
    {
        return new PayslipOverride(Code, Label, Location, Discount, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional tax form estimate is in its life cycle.</summary>
public enum RegionalTaxFormEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional tax form estimate as the tax form team keeps it.</summary>
public sealed class RegionalTaxFormEstimate(string code, string label, string unit, string department, string notes, double amount, RegionalTaxFormEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Department { get; set; } = department;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public RegionalTaxFormEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a regional tax form estimate in status draft.</summary>
    public static RegionalTaxFormEstimate NewRegionalTaxFormEstimate(string code, string label)
    {
        return new RegionalTaxFormEstimate(code, label, "", "", "", 0.0, RegionalTaxFormEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTaxFormEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTaxFormEstimateStatus next)
    {
        switch (Status)
        {
            case RegionalTaxFormEstimateStatus.Draft:
                return next == RegionalTaxFormEstimateStatus.Pending || next == RegionalTaxFormEstimateStatus.Closed;
            case RegionalTaxFormEstimateStatus.Pending:
                return next == RegionalTaxFormEstimateStatus.Active;
            case RegionalTaxFormEstimateStatus.Active:
                return next == RegionalTaxFormEstimateStatus.Suspended;
            case RegionalTaxFormEstimateStatus.Suspended:
                return next == RegionalTaxFormEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalTaxFormEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional tax form estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional tax form estimate"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RegionalTaxFormEstimate Copy()
    {
        return new RegionalTaxFormEstimate(Code, Label, Unit, Department, Notes, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}

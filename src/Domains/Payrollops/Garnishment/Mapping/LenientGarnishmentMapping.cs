namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient garnishment mapping is in its life cycle.</summary>
public enum LenientGarnishmentMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient garnishment mapping. Its code is unique within a tenant.</summary>
public sealed class LenientGarnishmentMapping(string code, string label, int tenureMonths, string department, string salaryBand, double budget, string managerId, double maximum, int quantity, string name, LenientGarnishmentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public string ManagerId { get; set; } = managerId;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public LenientGarnishmentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a lenient garnishment mapping in status draft.</summary>
    public static LenientGarnishmentMapping NewLenientGarnishmentMapping(string code, string label)
    {
        return new LenientGarnishmentMapping(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            LenientGarnishmentMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientGarnishmentMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientGarnishmentMappingStatus next)
    {
        switch (Status)
        {
            case LenientGarnishmentMappingStatus.Draft:
                switch (next)
                {
                    case LenientGarnishmentMappingStatus.Closed:
                        return true;
                }
                return false;
            case LenientGarnishmentMappingStatus.Pending:
                return next == LenientGarnishmentMappingStatus.Closed;
            case LenientGarnishmentMappingStatus.Active:
                switch (next)
                {
                    case LenientGarnishmentMappingStatus.Closed:
                        return true;
                }
                return false;
            case LenientGarnishmentMappingStatus.Suspended:
                return next == LenientGarnishmentMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientGarnishmentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient garnishment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Budget + Maximum, 2);
    }

    public LenientGarnishmentMapping Copy()
    {
        return new LenientGarnishmentMapping(
            Code,
            Label,
            TenureMonths,
            Department,
            SalaryBand,
            Budget,
            ManagerId,
            Maximum,
            Quantity,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}

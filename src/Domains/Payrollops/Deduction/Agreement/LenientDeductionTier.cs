namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction tier is in its life cycle.</summary>
public enum LenientDeductionTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient deduction tier. Its code is unique within a tenant.</summary>
public sealed class LenientDeductionTier(string code, string name, string employeeId, string phone, double threshold, bool approved, double margin, string managerId, string location, LenientDeductionTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public double Threshold { get; set; } = threshold;
    public bool Approved { get; set; } = approved;
    public double Margin { get; set; } = margin;
    public string ManagerId { get; set; } = managerId;
    public string Location { get; set; } = location;
    public LenientDeductionTierStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction tier in status created.</summary>
    public static LenientDeductionTier NewLenientDeductionTier(string code, string name)
    {
        return new LenientDeductionTier(
            code,
            name,
            "",
            "",
            0.0,
            false,
            0.0,
            "",
            "",
            LenientDeductionTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionTierStatus next)
    {
        switch (Status)
        {
            case LenientDeductionTierStatus.Created:
                switch (next)
                {
                    case LenientDeductionTierStatus.Validated:
                    case LenientDeductionTierStatus.Published:
                        return true;
                }
                return false;
            case LenientDeductionTierStatus.Validated:
                switch (next)
                {
                    case LenientDeductionTierStatus.Published:
                    case LenientDeductionTierStatus.Retired:
                        return true;
                }
                return false;
            case LenientDeductionTierStatus.Published:
                return next == LenientDeductionTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientDeductionTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Margin * Threshold, 2);
    }

    public LenientDeductionTier Copy()
    {
        return new LenientDeductionTier(
            Code,
            Name,
            EmployeeId,
            Phone,
            Threshold,
            Approved,
            Margin,
            ManagerId,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

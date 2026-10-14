namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay band agreement is in its life cycle.</summary>
public enum LegacyPayBandAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy pay band agreement, with what the planners need to act on it.</summary>
public sealed class LegacyPayBandAgreement(string code, string label, string salaryBand, double fte, string timezone, string country, string employeeId, string badge, string notes, string costCenter, LegacyPayBandAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public string Country { get; set; } = country;
    public string EmployeeId { get; set; } = employeeId;
    public string Badge { get; set; } = badge;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public LegacyPayBandAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay band agreement in status created.</summary>
    public static LegacyPayBandAgreement NewLegacyPayBandAgreement(string code, string label)
    {
        return new LegacyPayBandAgreement(
            code,
            label,
            "",
            0.0,
            "",
            "",
            "",
            "",
            "",
            "",
            LegacyPayBandAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayBandAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayBandAgreementStatus next)
    {
        switch (Status)
        {
            case LegacyPayBandAgreementStatus.Created:
                switch (next)
                {
                    case LegacyPayBandAgreementStatus.Published:
                        return true;
                }
                return false;
            case LegacyPayBandAgreementStatus.Validated:
                return next == LegacyPayBandAgreementStatus.Retired;
            case LegacyPayBandAgreementStatus.Published:
                switch (next)
                {
                    case LegacyPayBandAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayBandAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay band agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public LegacyPayBandAgreement Copy()
    {
        return new LegacyPayBandAgreement(
            Code,
            Label,
            SalaryBand,
            Fte,
            Timezone,
            Country,
            EmployeeId,
            Badge,
            Notes,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

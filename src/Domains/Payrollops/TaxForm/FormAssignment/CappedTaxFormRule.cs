namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped tax form rule is in its life cycle.</summary>
public enum CappedTaxFormRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a capped tax form rule and its current status.</summary>
public sealed class CappedTaxFormRule(string code, string name, string country, string checksum, string grade, string salaryBand, double hours, string ownerId, string phone, int expiresAt, int leaveDays, CappedTaxFormRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public string Grade { get; set; } = grade;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public int ExpiresAt { get; set; } = expiresAt;
    public int LeaveDays { get; set; } = leaveDays;
    public CappedTaxFormRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped tax form rule in status proposed.</summary>
    public static CappedTaxFormRule NewCappedTaxFormRule(string code, string name)
    {
        return new CappedTaxFormRule(
            code,
            name,
            "",
            "",
            "",
            "",
            0.0,
            "",
            "",
            0,
            0,
            CappedTaxFormRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTaxFormRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTaxFormRuleStatus next)
    {
        switch (Status)
        {
            case CappedTaxFormRuleStatus.Proposed:
                switch (next)
                {
                    case CappedTaxFormRuleStatus.Approved:
                    case CappedTaxFormRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedTaxFormRuleStatus.Approved:
                switch (next)
                {
                    case CappedTaxFormRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedTaxFormRuleStatus.Rejected:
                return next == CappedTaxFormRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedTaxFormRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped tax form rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays * (double)ExpiresAt, 2);
    }

    public CappedTaxFormRule Copy()
    {
        return new CappedTaxFormRule(
            Code,
            Name,
            Country,
            Checksum,
            Grade,
            SalaryBand,
            Hours,
            OwnerId,
            Phone,
            ExpiresAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

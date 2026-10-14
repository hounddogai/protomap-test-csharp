namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary skill profile manifest is in its life cycle.</summary>
public enum PrimarySkillProfileManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary skill profile manifest and its current status.</summary>
public sealed class PrimarySkillProfileManifest(string code, string label, string currency, string salaryBand, bool visible, string category, string employeeId, int endsAt, string country, string externalRef, PrimarySkillProfileManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Visible { get; set; } = visible;
    public string Category { get; set; } = category;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public string Country { get; set; } = country;
    public string ExternalRef { get; set; } = externalRef;
    public PrimarySkillProfileManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary skill profile manifest in status draft.</summary>
    public static PrimarySkillProfileManifest NewPrimarySkillProfileManifest(string code, string label)
    {
        return new PrimarySkillProfileManifest(
            code,
            label,
            "",
            "",
            false,
            "",
            "",
            0,
            "",
            "",
            PrimarySkillProfileManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySkillProfileManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySkillProfileManifestStatus next)
    {
        switch (Status)
        {
            case PrimarySkillProfileManifestStatus.Draft:
                return next == PrimarySkillProfileManifestStatus.Pending || next == PrimarySkillProfileManifestStatus.Suspended;
            case PrimarySkillProfileManifestStatus.Pending:
                switch (next)
                {
                    case PrimarySkillProfileManifestStatus.Active:
                    case PrimarySkillProfileManifestStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySkillProfileManifestStatus.Active:
                switch (next)
                {
                    case PrimarySkillProfileManifestStatus.Suspended:
                    case PrimarySkillProfileManifestStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySkillProfileManifestStatus.Suspended:
                return next == PrimarySkillProfileManifestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimarySkillProfileManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary skill profile manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary skill profile manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public PrimarySkillProfileManifest Copy()
    {
        return new PrimarySkillProfileManifest(
            Code,
            Label,
            Currency,
            SalaryBand,
            Visible,
            Category,
            EmployeeId,
            EndsAt,
            Country,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}

namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional career path quota is in its life cycle.</summary>
public enum RegionalCareerPathQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional career path quota. Its code is unique within a tenant.</summary>
public sealed class RegionalCareerPathQuota(string code, string label, string locale, double score, bool archived, string category, string region, string employeeId, RegionalCareerPathQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public RegionalCareerPathQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a regional career path quota in status created.</summary>
    public static RegionalCareerPathQuota NewRegionalCareerPathQuota(string code, string label)
    {
        return new RegionalCareerPathQuota(
            code,
            label,
            "",
            0.0,
            false,
            "",
            "",
            "",
            RegionalCareerPathQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCareerPathQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCareerPathQuotaStatus next)
    {
        switch (Status)
        {
            case RegionalCareerPathQuotaStatus.Created:
                switch (next)
                {
                    case RegionalCareerPathQuotaStatus.Validated:
                        return true;
                }
                return false;
            case RegionalCareerPathQuotaStatus.Validated:
                switch (next)
                {
                    case RegionalCareerPathQuotaStatus.Published:
                    case RegionalCareerPathQuotaStatus.Retired:
                        return true;
                }
                return false;
            case RegionalCareerPathQuotaStatus.Published:
                return next == RegionalCareerPathQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalCareerPathQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional career path quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RegionalCareerPathQuota Copy()
    {
        return new RegionalCareerPathQuota(
            Code,
            Label,
            Locale,
            Score,
            Archived,
            Category,
            Region,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

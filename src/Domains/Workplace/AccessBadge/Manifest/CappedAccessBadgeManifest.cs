namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped access badge manifest is in its life cycle.</summary>
public enum CappedAccessBadgeManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped access badge manifest as the access badge team keeps it.</summary>
public sealed class CappedAccessBadgeManifest(string code, string title, int retries, bool active, string phone, string category, List<string> tags, int windowDays, string salaryBand, string currency, bool visible, CappedAccessBadgeManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public string Currency { get; set; } = currency;
    public bool Visible { get; set; } = visible;
    public CappedAccessBadgeManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped access badge manifest in status created.</summary>
    public static CappedAccessBadgeManifest NewCappedAccessBadgeManifest(string code, string title)
    {
        return new CappedAccessBadgeManifest(
            code,
            title,
            0,
            false,
            "",
            "",
            new List<string>(),
            0,
            "",
            "",
            false,
            CappedAccessBadgeManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccessBadgeManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccessBadgeManifestStatus next)
    {
        switch (Status)
        {
            case CappedAccessBadgeManifestStatus.Created:
                return next == CappedAccessBadgeManifestStatus.Validated || next == CappedAccessBadgeManifestStatus.Retired;
            case CappedAccessBadgeManifestStatus.Validated:
                return next == CappedAccessBadgeManifestStatus.Published || next == CappedAccessBadgeManifestStatus.Retired;
            case CappedAccessBadgeManifestStatus.Published:
                switch (next)
                {
                    case CappedAccessBadgeManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAccessBadgeManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped access badge manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped access badge manifest"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedExposure()
    {
        return Math.Round((double)WindowDays - (double)Retries, 2);
    }

    public CappedAccessBadgeManifest Copy()
    {
        return new CappedAccessBadgeManifest(
            Code,
            Title,
            Retries,
            Active,
            Phone,
            Category,
            Tags,
            WindowDays,
            SalaryBand,
            Currency,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}

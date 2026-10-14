namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped access badge mapping is in its life cycle.</summary>
public enum CappedAccessBadgeMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped access badge mapping, with what the planners need to act on it.</summary>
public sealed class CappedAccessBadgeMapping(string code, string name, string checksum, double fte, int attempts, string reviewer, string salaryBand, List<string> tags, string label, double score, string phone, CappedAccessBadgeMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public string SalaryBand { get; set; } = salaryBand;
    public List<string> Tags { get; set; } = tags;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Phone { get; set; } = phone;
    public CappedAccessBadgeMappingStatus Status { get; set; } = status;

    /// <summary>Starts a capped access badge mapping in status draft.</summary>
    public static CappedAccessBadgeMapping NewCappedAccessBadgeMapping(string code, string name)
    {
        return new CappedAccessBadgeMapping(
            code,
            name,
            "",
            0.0,
            0,
            "",
            "",
            new List<string>(),
            "",
            0.0,
            "",
            CappedAccessBadgeMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccessBadgeMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccessBadgeMappingStatus next)
    {
        switch (Status)
        {
            case CappedAccessBadgeMappingStatus.Draft:
                return next == CappedAccessBadgeMappingStatus.Suspended;
            case CappedAccessBadgeMappingStatus.Pending:
                switch (next)
                {
                    case CappedAccessBadgeMappingStatus.Suspended:
                    case CappedAccessBadgeMappingStatus.Closed:
                        return true;
                }
                return false;
            case CappedAccessBadgeMappingStatus.Active:
                return next == CappedAccessBadgeMappingStatus.Suspended || next == CappedAccessBadgeMappingStatus.Closed;
            case CappedAccessBadgeMappingStatus.Suspended:
                switch (next)
                {
                    case CappedAccessBadgeMappingStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAccessBadgeMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped access badge mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
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

    public double ProjectedTotal()
    {
        return Math.Round(Score + (double)Attempts, 2);
    }

    public CappedAccessBadgeMapping Copy()
    {
        return new CappedAccessBadgeMapping(
            Code,
            Name,
            Checksum,
            Fte,
            Attempts,
            Reviewer,
            SalaryBand,
            Tags,
            Label,
            Score,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

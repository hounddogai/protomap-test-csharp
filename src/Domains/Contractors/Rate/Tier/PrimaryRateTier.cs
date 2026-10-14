namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary rate tier is in its life cycle.</summary>
public enum PrimaryRateTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary rate tier. Its code is unique within a tenant.</summary>
public sealed class PrimaryRateTier(string code, string title, int tenureMonths, string employeeId, string ownerId, string skill, Dictionary<string, string> attributes, string name, string locale, PrimaryRateTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public PrimaryRateTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary rate tier in status open.</summary>
    public static PrimaryRateTier NewPrimaryRateTier(string code, string title)
    {
        return new PrimaryRateTier(
            code,
            title,
            0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            PrimaryRateTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRateTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRateTierStatus next)
    {
        switch (Status)
        {
            case PrimaryRateTierStatus.Open:
                switch (next)
                {
                    case PrimaryRateTierStatus.InReview:
                        return true;
                }
                return false;
            case PrimaryRateTierStatus.InReview:
                return next == PrimaryRateTierStatus.Resolved || next == PrimaryRateTierStatus.Reopened;
            case PrimaryRateTierStatus.Resolved:
                switch (next)
                {
                    case PrimaryRateTierStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryRateTierStatus.Reopened:
                switch (next)
                {
                    case PrimaryRateTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryRateTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary rate tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public PrimaryRateTier Copy()
    {
        return new PrimaryRateTier(
            Code,
            Title,
            TenureMonths,
            EmployeeId,
            OwnerId,
            Skill,
            Attributes,
            Name,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

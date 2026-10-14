namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge rollup is in its life cycle.</summary>
public enum AccessBadgeRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A access badge rollup as the access badge team keeps it.</summary>
public sealed class AccessBadgeRollup(string code, string label, string phone, double hours, int revision, string country, int tenureMonths, int capacity, double rating, bool pinned, string reviewer, AccessBadgeRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public int Revision { get; set; } = revision;
    public string Country { get; set; } = country;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public bool Pinned { get; set; } = pinned;
    public string Reviewer { get; set; } = reviewer;
    public AccessBadgeRollupStatus Status { get; set; } = status;

    /// <summary>Starts a access badge rollup in status draft.</summary>
    public static AccessBadgeRollup NewAccessBadgeRollup(string code, string label)
    {
        return new AccessBadgeRollup(
            code,
            label,
            "",
            0.0,
            0,
            "",
            0,
            0,
            0.0,
            false,
            "",
            AccessBadgeRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeRollupStatus next)
    {
        switch (Status)
        {
            case AccessBadgeRollupStatus.Draft:
                switch (next)
                {
                    case AccessBadgeRollupStatus.Active:
                    case AccessBadgeRollupStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgeRollupStatus.Pending:
                return next == AccessBadgeRollupStatus.Active || next == AccessBadgeRollupStatus.Closed;
            case AccessBadgeRollupStatus.Active:
                return next == AccessBadgeRollupStatus.Suspended || next == AccessBadgeRollupStatus.Closed;
            case AccessBadgeRollupStatus.Suspended:
                return next == AccessBadgeRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Revision * Hours, 2);
    }

    public AccessBadgeRollup Copy()
    {
        return new AccessBadgeRollup(
            Code,
            Label,
            Phone,
            Hours,
            Revision,
            Country,
            TenureMonths,
            Capacity,
            Rating,
            Pinned,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge entry is in its life cycle.</summary>
public enum AccessBadgeEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One access badge entry, with what the planners need to act on it.</summary>
public sealed class AccessBadgeEntry(string code, string label, string team, bool active, double cost, int durationMinutes, string managerId, double amount, double rating, string salaryBand, AccessBadgeEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public double Rating { get; set; } = rating;
    public string SalaryBand { get; set; } = salaryBand;
    public AccessBadgeEntryStatus Status { get; set; } = status;

    /// <summary>Starts a access badge entry in status draft.</summary>
    public static AccessBadgeEntry NewAccessBadgeEntry(string code, string label)
    {
        return new AccessBadgeEntry(code, label, "", false, 0.0, 0, "", 0.0, 0.0, "", AccessBadgeEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeEntryStatus next)
    {
        switch (Status)
        {
            case AccessBadgeEntryStatus.Draft:
                switch (next)
                {
                    case AccessBadgeEntryStatus.Active:
                        return true;
                }
                return false;
            case AccessBadgeEntryStatus.Pending:
                switch (next)
                {
                    case AccessBadgeEntryStatus.Active:
                    case AccessBadgeEntryStatus.Suspended:
                        return true;
                }
                return false;
            case AccessBadgeEntryStatus.Active:
                return next == AccessBadgeEntryStatus.Suspended || next == AccessBadgeEntryStatus.Closed;
            case AccessBadgeEntryStatus.Suspended:
                return next == AccessBadgeEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Rating * Amount, 2);
    }

    public AccessBadgeEntry Copy()
    {
        return new AccessBadgeEntry(
            Code,
            Label,
            Team,
            Active,
            Cost,
            DurationMinutes,
            ManagerId,
            Amount,
            Rating,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

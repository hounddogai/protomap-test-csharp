namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge tier is in its life cycle.</summary>
public enum AccessBadgeTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A access badge tier. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeTier(string code, string name, int leaveDays, int limit, bool active, int batchSize, string region, int capacity, AccessBadgeTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public bool Active { get; set; } = active;
    public int BatchSize { get; set; } = batchSize;
    public string Region { get; set; } = region;
    public int Capacity { get; set; } = capacity;
    public AccessBadgeTierStatus Status { get; set; } = status;

    /// <summary>Starts a access badge tier in status created.</summary>
    public static AccessBadgeTier NewAccessBadgeTier(string code, string name)
    {
        return new AccessBadgeTier(code, name, 0, 0, false, 0, "", 0, AccessBadgeTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeTierStatus next)
    {
        switch (Status)
        {
            case AccessBadgeTierStatus.Created:
                return next == AccessBadgeTierStatus.Retired;
            case AccessBadgeTierStatus.Validated:
                switch (next)
                {
                    case AccessBadgeTierStatus.Published:
                        return true;
                }
                return false;
            case AccessBadgeTierStatus.Published:
                switch (next)
                {
                    case AccessBadgeTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)BatchSize * (double)Limit, 2);
    }

    public AccessBadgeTier Copy()
    {
        return new AccessBadgeTier(Code, Name, LeaveDays, Limit, Active, BatchSize, Region, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}

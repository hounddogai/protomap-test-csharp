namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge digest is in its life cycle.</summary>
public enum AccessBadgeDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A access badge digest. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeDigest(string code, string name, int capacity, double hours, bool pinned, string skill, string summary, string costCenter, int startsAt, bool visible, AccessBadgeDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Hours { get; set; } = hours;
    public bool Pinned { get; set; } = pinned;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public bool Visible { get; set; } = visible;
    public AccessBadgeDigestStatus Status { get; set; } = status;

    /// <summary>Starts a access badge digest in status proposed.</summary>
    public static AccessBadgeDigest NewAccessBadgeDigest(string code, string name)
    {
        return new AccessBadgeDigest(
            code,
            name,
            0,
            0.0,
            false,
            "",
            "",
            "",
            0,
            false,
            AccessBadgeDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeDigestStatus next)
    {
        switch (Status)
        {
            case AccessBadgeDigestStatus.Proposed:
                return next == AccessBadgeDigestStatus.Approved || next == AccessBadgeDigestStatus.Rejected;
            case AccessBadgeDigestStatus.Approved:
                switch (next)
                {
                    case AccessBadgeDigestStatus.Rejected:
                    case AccessBadgeDigestStatus.Withdrawn:
                        return true;
                }
                return false;
            case AccessBadgeDigestStatus.Rejected:
                return next == AccessBadgeDigestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AccessBadgeDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)StartsAt - Hours, 2);
    }

    public AccessBadgeDigest Copy()
    {
        return new AccessBadgeDigest(
            Code,
            Name,
            Capacity,
            Hours,
            Pinned,
            Skill,
            Summary,
            CostCenter,
            StartsAt,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

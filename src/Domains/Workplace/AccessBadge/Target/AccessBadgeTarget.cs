namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge target is in its life cycle.</summary>
public enum AccessBadgeTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One access badge target, with what the planners need to act on it.</summary>
public sealed class AccessBadgeTarget(string code, string name, string region, int attempts, double weight, AccessBadgeTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public AccessBadgeTargetStatus Status { get; set; } = status;

    /// <summary>Starts a access badge target in status draft.</summary>
    public static AccessBadgeTarget NewAccessBadgeTarget(string code, string name)
    {
        return new AccessBadgeTarget(code, name, "", 0, 0.0, AccessBadgeTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeTargetStatus next)
    {
        switch (Status)
        {
            case AccessBadgeTargetStatus.Draft:
                return next == AccessBadgeTargetStatus.Active || next == AccessBadgeTargetStatus.Closed;
            case AccessBadgeTargetStatus.Pending:
                switch (next)
                {
                    case AccessBadgeTargetStatus.Active:
                    case AccessBadgeTargetStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgeTargetStatus.Active:
                return next == AccessBadgeTargetStatus.Closed;
            case AccessBadgeTargetStatus.Suspended:
                return next == AccessBadgeTargetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts + Weight, 2);
    }

    public AccessBadgeTarget Copy()
    {
        return new AccessBadgeTarget(Code, Name, Region, Attempts, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}

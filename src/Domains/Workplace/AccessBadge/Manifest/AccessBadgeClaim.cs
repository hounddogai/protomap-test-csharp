namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge claim is in its life cycle.</summary>
public enum AccessBadgeClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One access badge claim, with what the planners need to act on it.</summary>
public sealed class AccessBadgeClaim(string code, string name, double margin, double minimum, int leaveDays, string team, string contactEmail, double cost, AccessBadgeClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public double Cost { get; set; } = cost;
    public AccessBadgeClaimStatus Status { get; set; } = status;

    /// <summary>Starts a access badge claim in status created.</summary>
    public static AccessBadgeClaim NewAccessBadgeClaim(string code, string name)
    {
        return new AccessBadgeClaim(code, name, 0.0, 0.0, 0, "", "", 0.0, AccessBadgeClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeClaimStatus next)
    {
        switch (Status)
        {
            case AccessBadgeClaimStatus.Created:
                switch (next)
                {
                    case AccessBadgeClaimStatus.Published:
                    case AccessBadgeClaimStatus.Retired:
                        return true;
                }
                return false;
            case AccessBadgeClaimStatus.Validated:
                switch (next)
                {
                    case AccessBadgeClaimStatus.Published:
                    case AccessBadgeClaimStatus.Retired:
                        return true;
                }
                return false;
            case AccessBadgeClaimStatus.Published:
                return next == AccessBadgeClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AccessBadgeClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Margin - Cost, 2);
    }

    public AccessBadgeClaim Copy()
    {
        return new AccessBadgeClaim(Code, Name, Margin, Minimum, LeaveDays, Team, ContactEmail, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

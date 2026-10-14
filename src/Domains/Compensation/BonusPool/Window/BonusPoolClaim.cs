namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool claim is in its life cycle.</summary>
public enum BonusPoolClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A bonus pool claim as the bonus pool team keeps it.</summary>
public sealed class BonusPoolClaim(string code, string name, string skill, double maximum, string salaryBand, int expiresAt, int version, BonusPoolClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public BonusPoolClaimStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool claim in status open.</summary>
    public static BonusPoolClaim NewBonusPoolClaim(string code, string name)
    {
        return new BonusPoolClaim(code, name, "", 0.0, "", 0, 0, BonusPoolClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolClaimStatus next)
    {
        switch (Status)
        {
            case BonusPoolClaimStatus.Open:
                switch (next)
                {
                    case BonusPoolClaimStatus.Resolved:
                    case BonusPoolClaimStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolClaimStatus.InReview:
                return next == BonusPoolClaimStatus.Resolved;
            case BonusPoolClaimStatus.Resolved:
                switch (next)
                {
                    case BonusPoolClaimStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolClaimStatus.Reopened:
                switch (next)
                {
                    case BonusPoolClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)ExpiresAt + Maximum, 2);
    }

    public BonusPoolClaim Copy()
    {
        return new BonusPoolClaim(Code, Name, Skill, Maximum, SalaryBand, ExpiresAt, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback bonus pool milestone is in its life cycle.</summary>
public enum FallbackBonusPoolMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback bonus pool milestone. Its code is unique within a tenant.</summary>
public sealed class FallbackBonusPoolMilestone(string code, string name, bool archived, double discount, int capacity, int expiresAt, string managerId, string externalRef, string timezone, string skill, FallbackBonusPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public FallbackBonusPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a fallback bonus pool milestone in status open.</summary>
    public static FallbackBonusPoolMilestone NewFallbackBonusPoolMilestone(string code, string name)
    {
        return new FallbackBonusPoolMilestone(
            code,
            name,
            false,
            0.0,
            0,
            0,
            "",
            "",
            "",
            "",
            FallbackBonusPoolMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBonusPoolMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBonusPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case FallbackBonusPoolMilestoneStatus.Open:
                switch (next)
                {
                    case FallbackBonusPoolMilestoneStatus.Resolved:
                    case FallbackBonusPoolMilestoneStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackBonusPoolMilestoneStatus.InReview:
                return next == FallbackBonusPoolMilestoneStatus.Resolved || next == FallbackBonusPoolMilestoneStatus.Archived;
            case FallbackBonusPoolMilestoneStatus.Resolved:
                switch (next)
                {
                    case FallbackBonusPoolMilestoneStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackBonusPoolMilestoneStatus.Reopened:
                return next == FallbackBonusPoolMilestoneStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackBonusPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback bonus pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Capacity - Discount, 2);
    }

    public FallbackBonusPoolMilestone Copy()
    {
        return new FallbackBonusPoolMilestone(
            Code,
            Name,
            Archived,
            Discount,
            Capacity,
            ExpiresAt,
            ManagerId,
            ExternalRef,
            Timezone,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

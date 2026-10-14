namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile tier is in its life cycle.</summary>
public enum SkillProfileTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A skill profile tier. Its code is unique within a tenant.</summary>
public sealed class SkillProfileTier(string code, string name, int priority, string category, int leaveDays, double fte, string currency, string costCenter, string badge, SkillProfileTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string Category { get; set; } = category;
    public int LeaveDays { get; set; } = leaveDays;
    public double Fte { get; set; } = fte;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public SkillProfileTierStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile tier in status draft.</summary>
    public static SkillProfileTier NewSkillProfileTier(string code, string name)
    {
        return new SkillProfileTier(code, name, 0, "", 0, 0.0, "", "", "", SkillProfileTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileTierStatus next)
    {
        switch (Status)
        {
            case SkillProfileTierStatus.Draft:
                return next == SkillProfileTierStatus.Pending;
            case SkillProfileTierStatus.Pending:
                switch (next)
                {
                    case SkillProfileTierStatus.Active:
                        return true;
                }
                return false;
            case SkillProfileTierStatus.Active:
                switch (next)
                {
                    case SkillProfileTierStatus.Suspended:
                        return true;
                }
                return false;
            case SkillProfileTierStatus.Suspended:
                switch (next)
                {
                    case SkillProfileTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Fte - (double)LeaveDays, 2);
    }

    public SkillProfileTier Copy()
    {
        return new SkillProfileTier(
            Code,
            Name,
            Priority,
            Category,
            LeaveDays,
            Fte,
            Currency,
            CostCenter,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}

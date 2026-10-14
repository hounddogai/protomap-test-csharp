namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile rule is in its life cycle.</summary>
public enum SkillProfileRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A skill profile rule. Its code is unique within a tenant.</summary>
public sealed class SkillProfileRule(string code, string name, string grade, int updatedAt, int tenureMonths, SkillProfileRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public SkillProfileRuleStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile rule in status open.</summary>
    public static SkillProfileRule NewSkillProfileRule(string code, string name)
    {
        return new SkillProfileRule(code, name, "", 0, 0, SkillProfileRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileRuleStatus next)
    {
        switch (Status)
        {
            case SkillProfileRuleStatus.Open:
                switch (next)
                {
                    case SkillProfileRuleStatus.Resolved:
                    case SkillProfileRuleStatus.Reopened:
                        return true;
                }
                return false;
            case SkillProfileRuleStatus.InReview:
                return next == SkillProfileRuleStatus.Reopened || next == SkillProfileRuleStatus.Archived;
            case SkillProfileRuleStatus.Resolved:
                switch (next)
                {
                    case SkillProfileRuleStatus.Reopened:
                    case SkillProfileRuleStatus.Archived:
                        return true;
                }
                return false;
            case SkillProfileRuleStatus.Reopened:
                switch (next)
                {
                    case SkillProfileRuleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile rule"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt - (double)TenureMonths, 2);
    }

    public SkillProfileRule Copy()
    {
        return new SkillProfileRule(Code, Name, Grade, UpdatedAt, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

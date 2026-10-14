namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient career path rule is in its life cycle.</summary>
public enum LenientCareerPathRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient career path rule, with what the planners need to act on it.</summary>
public sealed class LenientCareerPathRule(string code, string name, string source, string skill, int createdAt, string managerId, string channel, double rate, bool approved, LenientCareerPathRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public double Rate { get; set; } = rate;
    public bool Approved { get; set; } = approved;
    public LenientCareerPathRuleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient career path rule in status open.</summary>
    public static LenientCareerPathRule NewLenientCareerPathRule(string code, string name)
    {
        return new LenientCareerPathRule(
            code,
            name,
            "",
            "",
            0,
            "",
            "",
            0.0,
            false,
            LenientCareerPathRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCareerPathRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCareerPathRuleStatus next)
    {
        switch (Status)
        {
            case LenientCareerPathRuleStatus.Open:
                return next == LenientCareerPathRuleStatus.Resolved || next == LenientCareerPathRuleStatus.Archived;
            case LenientCareerPathRuleStatus.InReview:
                switch (next)
                {
                    case LenientCareerPathRuleStatus.Reopened:
                    case LenientCareerPathRuleStatus.Archived:
                        return true;
                }
                return false;
            case LenientCareerPathRuleStatus.Resolved:
                switch (next)
                {
                    case LenientCareerPathRuleStatus.Reopened:
                        return true;
                }
                return false;
            case LenientCareerPathRuleStatus.Reopened:
                switch (next)
                {
                    case LenientCareerPathRuleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientCareerPathRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient career path rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient career path rule"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)CreatedAt * Rate, 2);
    }

    public LenientCareerPathRule Copy()
    {
        return new LenientCareerPathRule(
            Code,
            Name,
            Source,
            Skill,
            CreatedAt,
            ManagerId,
            Channel,
            Rate,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}

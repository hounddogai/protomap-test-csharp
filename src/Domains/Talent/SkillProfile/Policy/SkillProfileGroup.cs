namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile group is in its life cycle.</summary>
public enum SkillProfileGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A skill profile group as the skill profile team keeps it.</summary>
public sealed class SkillProfileGroup(string code, string name, double maximum, string reviewer, int limit, SkillProfileGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public SkillProfileGroupStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile group in status open.</summary>
    public static SkillProfileGroup NewSkillProfileGroup(string code, string name)
    {
        return new SkillProfileGroup(code, name, 0.0, "", 0, SkillProfileGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileGroupStatus next)
    {
        switch (Status)
        {
            case SkillProfileGroupStatus.Open:
                return next == SkillProfileGroupStatus.InReview;
            case SkillProfileGroupStatus.InReview:
                return next == SkillProfileGroupStatus.Resolved || next == SkillProfileGroupStatus.Reopened;
            case SkillProfileGroupStatus.Resolved:
                return next == SkillProfileGroupStatus.Reopened || next == SkillProfileGroupStatus.Archived;
            case SkillProfileGroupStatus.Reopened:
                switch (next)
                {
                    case SkillProfileGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"skill profile group"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit - Maximum, 2);
    }

    public SkillProfileGroup Copy()
    {
        return new SkillProfileGroup(Code, Name, Maximum, Reviewer, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

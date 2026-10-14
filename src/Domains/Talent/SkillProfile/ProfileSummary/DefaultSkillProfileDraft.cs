namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile draft is in its life cycle.</summary>
public enum DefaultSkillProfileDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A default skill profile draft as the skill profile team keeps it.</summary>
public sealed class DefaultSkillProfileDraft(string code, string name, string department, Dictionary<string, string> attributes, double budget, string skill, DefaultSkillProfileDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public DefaultSkillProfileDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile draft in status requested.</summary>
    public static DefaultSkillProfileDraft NewDefaultSkillProfileDraft(string code, string name)
    {
        return new DefaultSkillProfileDraft(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            DefaultSkillProfileDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileDraftStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileDraftStatus.Requested:
                return next == DefaultSkillProfileDraftStatus.Revoked || next == DefaultSkillProfileDraftStatus.Expired;
            case DefaultSkillProfileDraftStatus.Granted:
                return next == DefaultSkillProfileDraftStatus.Expired;
            case DefaultSkillProfileDraftStatus.Revoked:
                return next == DefaultSkillProfileDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public DefaultSkillProfileDraft Copy()
    {
        return new DefaultSkillProfileDraft(Code, Name, Department, Attributes, Budget, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}

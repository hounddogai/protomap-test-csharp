namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped skill profile mapping is in its life cycle.</summary>
public enum CappedSkillProfileMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped skill profile mapping. Its code is unique within a tenant.</summary>
public sealed class CappedSkillProfileMapping(string code, string title, double discount, string category, bool active, string skill, CappedSkillProfileMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public CappedSkillProfileMappingStatus Status { get; set; } = status;

    /// <summary>Starts a capped skill profile mapping in status planned.</summary>
    public static CappedSkillProfileMapping NewCappedSkillProfileMapping(string code, string title)
    {
        return new CappedSkillProfileMapping(
            code,
            title,
            0.0,
            "",
            false,
            "",
            CappedSkillProfileMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSkillProfileMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSkillProfileMappingStatus next)
    {
        switch (Status)
        {
            case CappedSkillProfileMappingStatus.Planned:
                switch (next)
                {
                    case CappedSkillProfileMappingStatus.Scheduled:
                    case CappedSkillProfileMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedSkillProfileMappingStatus.Scheduled:
                switch (next)
                {
                    case CappedSkillProfileMappingStatus.InProgress:
                    case CappedSkillProfileMappingStatus.Completed:
                        return true;
                }
                return false;
            case CappedSkillProfileMappingStatus.InProgress:
                switch (next)
                {
                    case CappedSkillProfileMappingStatus.Completed:
                    case CappedSkillProfileMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedSkillProfileMappingStatus.Completed:
                return next == CappedSkillProfileMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedSkillProfileMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped skill profile mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public CappedSkillProfileMapping Copy()
    {
        return new CappedSkillProfileMapping(Code, Title, Discount, Category, Active, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}

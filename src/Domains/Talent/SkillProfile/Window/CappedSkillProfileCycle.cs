namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped skill profile cycle is in its life cycle.</summary>
public enum CappedSkillProfileCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a capped skill profile cycle and its current status.</summary>
public sealed class CappedSkillProfileCycle(string code, string title, string costCenter, string description, string ownerId, bool active, string skill, string unit, int createdAt, string region, CappedSkillProfileCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public string OwnerId { get; set; } = ownerId;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public CappedSkillProfileCycleStatus Status { get; set; } = status;

    /// <summary>Starts a capped skill profile cycle in status proposed.</summary>
    public static CappedSkillProfileCycle NewCappedSkillProfileCycle(string code, string title)
    {
        return new CappedSkillProfileCycle(
            code,
            title,
            "",
            "",
            "",
            false,
            "",
            "",
            0,
            "",
            CappedSkillProfileCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSkillProfileCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSkillProfileCycleStatus next)
    {
        switch (Status)
        {
            case CappedSkillProfileCycleStatus.Proposed:
                return next == CappedSkillProfileCycleStatus.Approved || next == CappedSkillProfileCycleStatus.Withdrawn;
            case CappedSkillProfileCycleStatus.Approved:
                return next == CappedSkillProfileCycleStatus.Rejected;
            case CappedSkillProfileCycleStatus.Rejected:
                return next == CappedSkillProfileCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedSkillProfileCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped skill profile cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public CappedSkillProfileCycle Copy()
    {
        return new CappedSkillProfileCycle(
            Code,
            Title,
            CostCenter,
            Description,
            OwnerId,
            Active,
            Skill,
            Unit,
            CreatedAt,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}

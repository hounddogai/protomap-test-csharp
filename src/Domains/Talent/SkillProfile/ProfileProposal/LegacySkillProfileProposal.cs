namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy skill profile proposal is in its life cycle.</summary>
public enum LegacySkillProfileProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy skill profile proposal as the skill profile team keeps it.</summary>
public sealed class LegacySkillProfileProposal(string code, string title, string employeeId, string description, double budget, double maximum, int capacity, LegacySkillProfileProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Description { get; set; } = description;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public int Capacity { get; set; } = capacity;
    public LegacySkillProfileProposalStatus Status { get; set; } = status;

    /// <summary>Starts a legacy skill profile proposal in status proposed.</summary>
    public static LegacySkillProfileProposal NewLegacySkillProfileProposal(string code, string title)
    {
        return new LegacySkillProfileProposal(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0,
            LegacySkillProfileProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySkillProfileProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySkillProfileProposalStatus next)
    {
        switch (Status)
        {
            case LegacySkillProfileProposalStatus.Proposed:
                return next == LegacySkillProfileProposalStatus.Approved || next == LegacySkillProfileProposalStatus.Rejected;
            case LegacySkillProfileProposalStatus.Approved:
                switch (next)
                {
                    case LegacySkillProfileProposalStatus.Rejected:
                    case LegacySkillProfileProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacySkillProfileProposalStatus.Rejected:
                return next == LegacySkillProfileProposalStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacySkillProfileProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy skill profile proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round(Budget - Maximum, 2);
    }

    public LegacySkillProfileProposal Copy()
    {
        return new LegacySkillProfileProposal(
            Code,
            Title,
            EmployeeId,
            Description,
            Budget,
            Maximum,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

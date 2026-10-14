namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled skill profile proposal is in its life cycle.</summary>
public enum ScheduledSkillProfileProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scheduled skill profile proposal and its current status.</summary>
public sealed class ScheduledSkillProfileProposal(string code, string label, double maximum, string employeeId, int priority, string contactEmail, string timezone, double ratio, ScheduledSkillProfileProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public ScheduledSkillProfileProposalStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled skill profile proposal in status planned.</summary>
    public static ScheduledSkillProfileProposal NewScheduledSkillProfileProposal(string code, string label)
    {
        return new ScheduledSkillProfileProposal(
            code,
            label,
            0.0,
            "",
            0,
            "",
            "",
            0.0,
            ScheduledSkillProfileProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSkillProfileProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSkillProfileProposalStatus next)
    {
        switch (Status)
        {
            case ScheduledSkillProfileProposalStatus.Planned:
                switch (next)
                {
                    case ScheduledSkillProfileProposalStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledSkillProfileProposalStatus.Scheduled:
                switch (next)
                {
                    case ScheduledSkillProfileProposalStatus.InProgress:
                    case ScheduledSkillProfileProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledSkillProfileProposalStatus.InProgress:
                switch (next)
                {
                    case ScheduledSkillProfileProposalStatus.Completed:
                    case ScheduledSkillProfileProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledSkillProfileProposalStatus.Completed:
                return next == ScheduledSkillProfileProposalStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledSkillProfileProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled skill profile proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum - (double)Priority, 2);
    }

    public ScheduledSkillProfileProposal Copy()
    {
        return new ScheduledSkillProfileProposal(
            Code,
            Label,
            Maximum,
            EmployeeId,
            Priority,
            ContactEmail,
            Timezone,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy skill profile checkpoint is in its life cycle.</summary>
public enum LegacySkillProfileCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy skill profile checkpoint. Its code is unique within a tenant.</summary>
public sealed class LegacySkillProfileCheckpoint(string code, string name, double margin, double hours, string grade, string notes, bool visible, string salaryBand, LegacySkillProfileCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public LegacySkillProfileCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy skill profile checkpoint in status planned.</summary>
    public static LegacySkillProfileCheckpoint NewLegacySkillProfileCheckpoint(string code, string name)
    {
        return new LegacySkillProfileCheckpoint(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            false,
            "",
            LegacySkillProfileCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySkillProfileCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySkillProfileCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacySkillProfileCheckpointStatus.Planned:
                return next == LegacySkillProfileCheckpointStatus.Scheduled || next == LegacySkillProfileCheckpointStatus.Cancelled;
            case LegacySkillProfileCheckpointStatus.Scheduled:
                return next == LegacySkillProfileCheckpointStatus.InProgress;
            case LegacySkillProfileCheckpointStatus.InProgress:
                return next == LegacySkillProfileCheckpointStatus.Cancelled;
            case LegacySkillProfileCheckpointStatus.Completed:
                return next == LegacySkillProfileCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacySkillProfileCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy skill profile checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy skill profile checkpoint"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Margin * Hours, 2);
    }

    public LegacySkillProfileCheckpoint Copy()
    {
        return new LegacySkillProfileCheckpoint(
            Code,
            Name,
            Margin,
            Hours,
            Grade,
            Notes,
            Visible,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

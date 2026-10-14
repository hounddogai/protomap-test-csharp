namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal skill profile cycle is in its life cycle.</summary>
public enum SeasonalSkillProfileCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal skill profile cycle. Its code is unique within a tenant.</summary>
public sealed class SeasonalSkillProfileCycle(string code, string label, double hours, bool active, bool pinned, double amount, double budget, int retries, int revision, int tenureMonths, int priority, SeasonalSkillProfileCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public bool Pinned { get; set; } = pinned;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public int Revision { get; set; } = revision;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public SeasonalSkillProfileCycleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal skill profile cycle in status new.</summary>
    public static SeasonalSkillProfileCycle NewSeasonalSkillProfileCycle(string code, string label)
    {
        return new SeasonalSkillProfileCycle(
            code,
            label,
            0.0,
            false,
            false,
            0.0,
            0.0,
            0,
            0,
            0,
            0,
            SeasonalSkillProfileCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSkillProfileCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSkillProfileCycleStatus next)
    {
        switch (Status)
        {
            case SeasonalSkillProfileCycleStatus.New:
                return next == SeasonalSkillProfileCycleStatus.Failed;
            case SeasonalSkillProfileCycleStatus.Queued:
                return next == SeasonalSkillProfileCycleStatus.Running || next == SeasonalSkillProfileCycleStatus.Done;
            case SeasonalSkillProfileCycleStatus.Running:
                switch (next)
                {
                    case SeasonalSkillProfileCycleStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalSkillProfileCycleStatus.Done:
                switch (next)
                {
                    case SeasonalSkillProfileCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSkillProfileCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal skill profile cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority * (double)Revision, 2);
    }

    public SeasonalSkillProfileCycle Copy()
    {
        return new SeasonalSkillProfileCycle(
            Code,
            Label,
            Hours,
            Active,
            Pinned,
            Amount,
            Budget,
            Retries,
            Revision,
            TenureMonths,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

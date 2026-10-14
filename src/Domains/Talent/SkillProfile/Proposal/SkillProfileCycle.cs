namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile cycle is in its life cycle.</summary>
public enum SkillProfileCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile cycle. Its code is unique within a tenant.</summary>
public sealed class SkillProfileCycle(string code, string name, double rate, int graceDays, double budget, int durationMinutes, string source, int capacity, string phone, int retries, double rating, SkillProfileCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Source { get; set; } = source;
    public int Capacity { get; set; } = capacity;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public double Rating { get; set; } = rating;
    public SkillProfileCycleStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile cycle in status planned.</summary>
    public static SkillProfileCycle NewSkillProfileCycle(string code, string name)
    {
        return new SkillProfileCycle(code, name, 0.0, 0, 0.0, 0, "", 0, "", 0, 0.0, SkillProfileCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileCycleStatus next)
    {
        switch (Status)
        {
            case SkillProfileCycleStatus.Planned:
                switch (next)
                {
                    case SkillProfileCycleStatus.Scheduled:
                    case SkillProfileCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileCycleStatus.Scheduled:
                return next == SkillProfileCycleStatus.Completed;
            case SkillProfileCycleStatus.InProgress:
                switch (next)
                {
                    case SkillProfileCycleStatus.Completed:
                    case SkillProfileCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileCycleStatus.Completed:
                switch (next)
                {
                    case SkillProfileCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Rate * (double)Capacity, 2);
    }

    public SkillProfileCycle Copy()
    {
        return new SkillProfileCycle(
            Code,
            Name,
            Rate,
            GraceDays,
            Budget,
            DurationMinutes,
            Source,
            Capacity,
            Phone,
            Retries,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}

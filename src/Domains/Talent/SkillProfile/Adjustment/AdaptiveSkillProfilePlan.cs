namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive skill profile plan is in its life cycle.</summary>
public enum AdaptiveSkillProfilePlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adaptive skill profile plan and its current status.</summary>
public sealed class AdaptiveSkillProfilePlan(string code, string name, int durationMinutes, int capacity, string description, string notes, string employeeId, AdaptiveSkillProfilePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public string EmployeeId { get; set; } = employeeId;
    public AdaptiveSkillProfilePlanStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive skill profile plan in status new.</summary>
    public static AdaptiveSkillProfilePlan NewAdaptiveSkillProfilePlan(string code, string name)
    {
        return new AdaptiveSkillProfilePlan(code, name, 0, 0, "", "", "", AdaptiveSkillProfilePlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSkillProfilePlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSkillProfilePlanStatus next)
    {
        switch (Status)
        {
            case AdaptiveSkillProfilePlanStatus.New:
                return next == AdaptiveSkillProfilePlanStatus.Running || next == AdaptiveSkillProfilePlanStatus.Done;
            case AdaptiveSkillProfilePlanStatus.Queued:
                return next == AdaptiveSkillProfilePlanStatus.Done;
            case AdaptiveSkillProfilePlanStatus.Running:
                switch (next)
                {
                    case AdaptiveSkillProfilePlanStatus.Done:
                    case AdaptiveSkillProfilePlanStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveSkillProfilePlanStatus.Done:
                switch (next)
                {
                    case AdaptiveSkillProfilePlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveSkillProfilePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive skill profile plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity - (double)DurationMinutes, 2);
    }

    public AdaptiveSkillProfilePlan Copy()
    {
        return new AdaptiveSkillProfilePlan(
            Code,
            Name,
            DurationMinutes,
            Capacity,
            Description,
            Notes,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

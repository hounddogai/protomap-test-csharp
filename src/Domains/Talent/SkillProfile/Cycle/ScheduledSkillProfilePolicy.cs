namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled skill profile policy is in its life cycle.</summary>
public enum ScheduledSkillProfilePolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled skill profile policy as the skill profile team keeps it.</summary>
public sealed class ScheduledSkillProfilePolicy(string code, string label, double rate, int graceDays, string segment, double ceiling, string jobCode, int updatedAt, double maximum, int tenureMonths, ScheduledSkillProfilePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public string Segment { get; set; } = segment;
    public double Ceiling { get; set; } = ceiling;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledSkillProfilePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled skill profile policy in status planned.</summary>
    public static ScheduledSkillProfilePolicy NewScheduledSkillProfilePolicy(string code, string label)
    {
        return new ScheduledSkillProfilePolicy(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            "",
            0,
            0.0,
            0,
            ScheduledSkillProfilePolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSkillProfilePolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSkillProfilePolicyStatus next)
    {
        switch (Status)
        {
            case ScheduledSkillProfilePolicyStatus.Planned:
                switch (next)
                {
                    case ScheduledSkillProfilePolicyStatus.Scheduled:
                    case ScheduledSkillProfilePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledSkillProfilePolicyStatus.Scheduled:
                switch (next)
                {
                    case ScheduledSkillProfilePolicyStatus.InProgress:
                    case ScheduledSkillProfilePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledSkillProfilePolicyStatus.InProgress:
                return next == ScheduledSkillProfilePolicyStatus.Completed || next == ScheduledSkillProfilePolicyStatus.Cancelled;
            case ScheduledSkillProfilePolicyStatus.Completed:
                switch (next)
                {
                    case ScheduledSkillProfilePolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSkillProfilePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled skill profile policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths * (double)UpdatedAt, 2);
    }

    public ScheduledSkillProfilePolicy Copy()
    {
        return new ScheduledSkillProfilePolicy(
            Code,
            Label,
            Rate,
            GraceDays,
            Segment,
            Ceiling,
            JobCode,
            UpdatedAt,
            Maximum,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

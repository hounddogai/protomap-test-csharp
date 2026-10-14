namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile policy is in its life cycle.</summary>
public enum SkillProfilePolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile policy as the skill profile team keeps it.</summary>
public sealed class SkillProfilePolicy(string code, string name, int graceDays, int priority, double amount, string jobCode, string contactEmail, string skill, SkillProfilePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public double Amount { get; set; } = amount;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public SkillProfilePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile policy in status planned.</summary>
    public static SkillProfilePolicy NewSkillProfilePolicy(string code, string name)
    {
        return new SkillProfilePolicy(code, name, 0, 0, 0.0, "", "", "", SkillProfilePolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfilePolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfilePolicyStatus next)
    {
        switch (Status)
        {
            case SkillProfilePolicyStatus.Planned:
                switch (next)
                {
                    case SkillProfilePolicyStatus.Completed:
                    case SkillProfilePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfilePolicyStatus.Scheduled:
                switch (next)
                {
                    case SkillProfilePolicyStatus.InProgress:
                        return true;
                }
                return false;
            case SkillProfilePolicyStatus.InProgress:
                return next == SkillProfilePolicyStatus.Cancelled;
            case SkillProfilePolicyStatus.Completed:
                return next == SkillProfilePolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfilePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"skill profile policy"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)GraceDays * (double)Priority, 2);
    }

    public SkillProfilePolicy Copy()
    {
        return new SkillProfilePolicy(Code, Name, GraceDays, Priority, Amount, JobCode, ContactEmail, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

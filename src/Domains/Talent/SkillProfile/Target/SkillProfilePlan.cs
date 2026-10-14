namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile plan is in its life cycle.</summary>
public enum SkillProfilePlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A skill profile plan as the skill profile team keeps it.</summary>
public sealed class SkillProfilePlan(string code, string label, string reviewer, double discount, double floorValue, SkillProfilePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public double Discount { get; set; } = discount;
    public double FloorValue { get; set; } = floorValue;
    public SkillProfilePlanStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile plan in status draft.</summary>
    public static SkillProfilePlan NewSkillProfilePlan(string code, string label)
    {
        return new SkillProfilePlan(code, label, "", 0.0, 0.0, SkillProfilePlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfilePlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfilePlanStatus next)
    {
        switch (Status)
        {
            case SkillProfilePlanStatus.Draft:
                switch (next)
                {
                    case SkillProfilePlanStatus.Pending:
                        return true;
                }
                return false;
            case SkillProfilePlanStatus.Pending:
                switch (next)
                {
                    case SkillProfilePlanStatus.Suspended:
                    case SkillProfilePlanStatus.Closed:
                        return true;
                }
                return false;
            case SkillProfilePlanStatus.Active:
                return next == SkillProfilePlanStatus.Suspended;
            case SkillProfilePlanStatus.Suspended:
                switch (next)
                {
                    case SkillProfilePlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfilePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round(Discount * FloorValue, 2);
    }

    public SkillProfilePlan Copy()
    {
        return new SkillProfilePlan(Code, Label, Reviewer, Discount, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

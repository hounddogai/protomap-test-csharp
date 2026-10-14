namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile template is in its life cycle.</summary>
public enum SkillProfileTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A skill profile template. Its code is unique within a tenant.</summary>
public sealed class SkillProfileTemplate(string code, string label, double cost, double score, string reviewer, SkillProfileTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public SkillProfileTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile template in status proposed.</summary>
    public static SkillProfileTemplate NewSkillProfileTemplate(string code, string label)
    {
        return new SkillProfileTemplate(code, label, 0.0, 0.0, "", SkillProfileTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileTemplateStatus next)
    {
        switch (Status)
        {
            case SkillProfileTemplateStatus.Proposed:
                switch (next)
                {
                    case SkillProfileTemplateStatus.Approved:
                        return true;
                }
                return false;
            case SkillProfileTemplateStatus.Approved:
                switch (next)
                {
                    case SkillProfileTemplateStatus.Rejected:
                    case SkillProfileTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case SkillProfileTemplateStatus.Rejected:
                switch (next)
                {
                    case SkillProfileTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"skill profile template"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost + Score, 2);
    }

    public SkillProfileTemplate Copy()
    {
        return new SkillProfileTemplate(Code, Label, Cost, Score, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

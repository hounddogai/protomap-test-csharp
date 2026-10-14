namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted skill profile budget is in its life cycle.</summary>
public enum WeightedSkillProfileBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted skill profile budget, with what the planners need to act on it.</summary>
public sealed class WeightedSkillProfileBudget(string code, string title, double fte, int leaveDays, double maximum, WeightedSkillProfileBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public double Maximum { get; set; } = maximum;
    public WeightedSkillProfileBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a weighted skill profile budget in status created.</summary>
    public static WeightedSkillProfileBudget NewWeightedSkillProfileBudget(string code, string title)
    {
        return new WeightedSkillProfileBudget(code, title, 0.0, 0, 0.0, WeightedSkillProfileBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSkillProfileBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSkillProfileBudgetStatus next)
    {
        switch (Status)
        {
            case WeightedSkillProfileBudgetStatus.Created:
                switch (next)
                {
                    case WeightedSkillProfileBudgetStatus.Published:
                    case WeightedSkillProfileBudgetStatus.Retired:
                        return true;
                }
                return false;
            case WeightedSkillProfileBudgetStatus.Validated:
                return next == WeightedSkillProfileBudgetStatus.Published || next == WeightedSkillProfileBudgetStatus.Retired;
            case WeightedSkillProfileBudgetStatus.Published:
                switch (next)
                {
                    case WeightedSkillProfileBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedSkillProfileBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted skill profile budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays - Maximum, 2);
    }

    public WeightedSkillProfileBudget Copy()
    {
        return new WeightedSkillProfileBudget(Code, Title, Fte, LeaveDays, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile draft policy.</summary>
public static class SkillProfileDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12856.0, in order.</summary>
    public static int SkillProfileDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12856.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SkillProfileDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

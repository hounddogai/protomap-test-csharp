namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending skill profile group policy.</summary>
public static class PendingSkillProfileGroupPolicy
{
    public static double PendingSkillProfileGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 21707.0, in order.</summary>
    public static int PendingSkillProfileGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21707.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global nomination milestone policy.</summary>
public static class GlobalNominationMilestonePolicy
{
    public static double GlobalNominationMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 52412.0, in order.</summary>
    public static int GlobalNominationMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52412.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GlobalNominationMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

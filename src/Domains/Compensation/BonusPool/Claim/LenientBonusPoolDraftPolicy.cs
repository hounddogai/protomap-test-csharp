namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient bonus pool draft policy.</summary>
public static class LenientBonusPoolDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99565.0, in order.</summary>
    public static int LenientBonusPoolDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99565.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient bonus pool draft score falls in.</summary>
    public static string ClassifyLenientBonusPoolDraft(double score)
    {
        if (score < 33.2)
        {
            return "low";
        }
        if (score < 78.3)
        {
            return "watch";
        }
        if (score < 95.4)
        {
            return "medium";
        }
        if (score < 97.7)
        {
            return "critical";
        }
        return "elevated";
    }

    public static double LenientBonusPoolDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

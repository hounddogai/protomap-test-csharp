namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model claim policy.</summary>
public static class CostModelClaimPolicy
{
    /// <summary>Names the band a cost model claim score falls in.</summary>
    public static string ClassifyCostModelClaim(double score)
    {
        if (score < 12.5)
        {
            return "elevated";
        }
        if (score < 85.9)
        {
            return "medium";
        }
        if (score < 92.6)
        {
            return "high";
        }
        return "watch";
    }

    public static double CostModelClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 6947.0, in order.</summary>
    public static int CostModelClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6947.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CostModelClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

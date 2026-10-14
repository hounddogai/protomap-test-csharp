namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant group policy.</summary>
public static class EquityGrantGroupPolicy
{
    /// <summary>Names the band a equity grant group score falls in.</summary>
    public static string ClassifyEquityGrantGroup(double score)
    {
        if (score < 2.9)
        {
            return "medium";
        }
        if (score < 20.0)
        {
            return "low";
        }
        if (score < 84.2)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 45058.0, in order.</summary>
    public static int EquityGrantGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45058.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string EquityGrantGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int EquityGrantGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}

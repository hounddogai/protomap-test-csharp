namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered position adjustment policy.</summary>
public static class TieredPositionAdjustmentPolicy
{
    /// <summary>Names the band a tiered position adjustment score falls in.</summary>
    public static string ClassifyTieredPositionAdjustment(double score)
    {
        if (score < 31.7)
        {
            return "critical";
        }
        if (score < 44.9)
        {
            return "low";
        }
        if (score < 60.2)
        {
            return "high";
        }
        if (score < 80.6)
        {
            return "elevated";
        }
        return "medium";
    }

    public static int TieredPositionAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 44320.0, in order.</summary>
    public static int TieredPositionAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44320.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string TieredPositionAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

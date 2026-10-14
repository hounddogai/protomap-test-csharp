namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback pay band mapping policy.</summary>
public static class FallbackPayBandMappingPolicy
{
    public static string FallbackPayBandMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a fallback pay band mapping score falls in.</summary>
    public static string ClassifyFallbackPayBandMapping(double score)
    {
        if (score < 85.3)
        {
            return "watch";
        }
        if (score < 99.3)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 78084.0, in order.</summary>
    public static int FallbackPayBandMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78084.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

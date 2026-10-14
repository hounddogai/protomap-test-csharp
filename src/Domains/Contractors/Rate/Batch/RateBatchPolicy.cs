namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate batch policy.</summary>
public static class RateBatchPolicy
{
    /// <summary>Names the band a rate batch score falls in.</summary>
    public static string ClassifyRateBatch(double score)
    {
        if (score < 31.0)
        {
            return "high";
        }
        if (score < 35.5)
        {
            return "critical";
        }
        if (score < 43.9)
        {
            return "watch";
        }
        return "minimal";
    }

    public static string RateBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72235.0, in order.</summary>
    public static int RateBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72235.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

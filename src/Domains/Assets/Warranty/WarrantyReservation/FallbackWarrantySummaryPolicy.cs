namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback warranty summary policy.</summary>
public static class FallbackWarrantySummaryPolicy
{
    /// <summary>Names the band a fallback warranty summary score falls in.</summary>
    public static string ClassifyFallbackWarrantySummary(double score)
    {
        if (score < 35.4)
        {
            return "ok";
        }
        if (score < 39.8)
        {
            return "low";
        }
        return "minimal";
    }

    public static string FallbackWarrantySummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 95028.0, in order.</summary>
    public static int FallbackWarrantySummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95028.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double FallbackWarrantySummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

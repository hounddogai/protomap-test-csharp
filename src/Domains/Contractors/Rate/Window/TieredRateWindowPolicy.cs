namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered rate window policy.</summary>
public static class TieredRateWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67874.0, in order.</summary>
    public static int TieredRateWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67874.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TieredRateWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string TieredRateWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double TieredRateWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

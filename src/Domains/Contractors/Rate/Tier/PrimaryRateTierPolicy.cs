namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary rate tier policy.</summary>
public static class PrimaryRateTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97409.0, in order.</summary>
    public static int PrimaryRateTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97409.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryRateTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryRateTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int PrimaryRateTierSpan(int start, int end)
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
        return span / 60;
    }
}

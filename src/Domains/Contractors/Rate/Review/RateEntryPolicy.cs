namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate entry policy.</summary>
public static class RateEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1470.0, in order.</summary>
    public static int RateEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1470.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RateEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RateEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered nomination forecast policy.</summary>
public static class TieredNominationForecastPolicy
{
    public static string TieredNominationForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double TieredNominationForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 63292.0, in order.</summary>
    public static int TieredNominationForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63292.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

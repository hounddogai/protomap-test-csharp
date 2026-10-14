namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall quota policy.</summary>
public static class TownHallQuotaPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93655.0, in order.</summary>
    public static int TownHallQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93655.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string TownHallQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double TownHallQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

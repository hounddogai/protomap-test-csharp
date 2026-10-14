namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual mobility reservation policy.</summary>
public static class ManualMobilityReservationPolicy
{
    public static string ManualMobilityReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double ManualMobilityReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 86281.0, in order.</summary>
    public static int ManualMobilityReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86281.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

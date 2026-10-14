namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination adjustment policy.</summary>
public static class NominationAdjustmentPolicy
{
    public static double NominationAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a nomination adjustment score falls in.</summary>
    public static string ClassifyNominationAdjustment(double score)
    {
        if (score < 38.8)
        {
            return "critical";
        }
        if (score < 41.7)
        {
            return "ok";
        }
        if (score < 60.2)
        {
            return "medium";
        }
        if (score < 75.6)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 2373.0, in order.</summary>
    public static int NominationAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2373.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

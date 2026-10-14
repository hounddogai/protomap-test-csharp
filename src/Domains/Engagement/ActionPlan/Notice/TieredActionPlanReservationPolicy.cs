namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered action plan reservation policy.</summary>
public static class TieredActionPlanReservationPolicy
{
    /// <summary>Names the band a tiered action plan reservation score falls in.</summary>
    public static string ClassifyTieredActionPlanReservation(double score)
    {
        if (score < 13.3)
        {
            return "high";
        }
        if (score < 31.4)
        {
            return "watch";
        }
        if (score < 43.6)
        {
            return "elevated";
        }
        if (score < 69.4)
        {
            return "low";
        }
        return "minimal";
    }

    public static string TieredActionPlanReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 21850.0, in order.</summary>
    public static int TieredActionPlanReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21850.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TieredActionPlanReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}

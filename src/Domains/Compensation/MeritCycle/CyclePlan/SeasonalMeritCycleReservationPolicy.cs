namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal merit cycle reservation policy.</summary>
public static class SeasonalMeritCycleReservationPolicy
{
    public static double SeasonalMeritCycleReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int SeasonalMeritCycleReservationSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a seasonal merit cycle reservation score falls in.</summary>
    public static string ClassifySeasonalMeritCycleReservation(double score)
    {
        if (score < 45.4)
        {
            return "ok";
        }
        if (score < 46.9)
        {
            return "high";
        }
        return "watch";
    }
}

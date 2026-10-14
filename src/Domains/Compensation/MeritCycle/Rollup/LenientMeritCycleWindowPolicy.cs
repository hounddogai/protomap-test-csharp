namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient merit cycle window policy.</summary>
public static class LenientMeritCycleWindowPolicy
{
    public static double LenientMeritCycleWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient merit cycle window score falls in.</summary>
    public static string ClassifyLenientMeritCycleWindow(double score)
    {
        if (score < 4.9)
        {
            return "medium";
        }
        if (score < 7.5)
        {
            return "low";
        }
        if (score < 33.1)
        {
            return "ok";
        }
        if (score < 49.9)
        {
            return "high";
        }
        return "watch";
    }
}

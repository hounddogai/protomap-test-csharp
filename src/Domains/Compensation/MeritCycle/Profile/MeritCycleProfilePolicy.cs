namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle profile policy.</summary>
public static class MeritCycleProfilePolicy
{
    public static double MeritCycleProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a merit cycle profile score falls in.</summary>
    public static string ClassifyMeritCycleProfile(double score)
    {
        if (score < 37.0)
        {
            return "watch";
        }
        if (score < 47.0)
        {
            return "ok";
        }
        if (score < 81.9)
        {
            return "medium";
        }
        return "low";
    }
}

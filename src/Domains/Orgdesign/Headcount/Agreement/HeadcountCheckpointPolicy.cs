namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount checkpoint policy.</summary>
public static class HeadcountCheckpointPolicy
{
    public static double HeadcountCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a headcount checkpoint score falls in.</summary>
    public static string ClassifyHeadcountCheckpoint(double score)
    {
        if (score < 9.0)
        {
            return "medium";
        }
        if (score < 15.8)
        {
            return "ok";
        }
        if (score < 25.9)
        {
            return "critical";
        }
        return "high";
    }
}

namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall preference policy.</summary>
public static class TownHallPreferencePolicy
{
    /// <summary>Names the band a town hall preference score falls in.</summary>
    public static string ClassifyTownHallPreference(double score)
    {
        if (score < 1.5)
        {
            return "critical";
        }
        if (score < 16.8)
        {
            return "elevated";
        }
        if (score < 66.7)
        {
            return "high";
        }
        return "low";
    }

    public static double TownHallPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

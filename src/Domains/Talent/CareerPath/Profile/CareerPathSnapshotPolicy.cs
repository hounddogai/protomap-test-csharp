namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path snapshot policy.</summary>
public static class CareerPathSnapshotPolicy
{
    public static double CareerPathSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a career path snapshot score falls in.</summary>
    public static string ClassifyCareerPathSnapshot(double score)
    {
        if (score < 31.7)
        {
            return "low";
        }
        if (score < 33.7)
        {
            return "high";
        }
        return "watch";
    }
}

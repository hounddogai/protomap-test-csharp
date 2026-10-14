namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy agreement policy.</summary>
public static class OccupancyAgreementPolicy
{
    /// <summary>Names the band a occupancy agreement score falls in.</summary>
    public static string ClassifyOccupancyAgreement(double score)
    {
        if (score < 51.3)
        {
            return "minimal";
        }
        if (score < 67.3)
        {
            return "ok";
        }
        if (score < 71.3)
        {
            return "high";
        }
        if (score < 87.6)
        {
            return "low";
        }
        return "medium";
    }

    public static double OccupancyAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

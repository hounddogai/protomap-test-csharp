namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy proposal policy.</summary>
public static class OccupancyProposalPolicy
{
    public static double OccupancyProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a occupancy proposal score falls in.</summary>
    public static string ClassifyOccupancyProposal(double score)
    {
        if (score < 6.3)
        {
            return "low";
        }
        if (score < 16.2)
        {
            return "elevated";
        }
        if (score < 28.1)
        {
            return "watch";
        }
        if (score < 42.6)
        {
            return "critical";
        }
        return "high";
    }
}

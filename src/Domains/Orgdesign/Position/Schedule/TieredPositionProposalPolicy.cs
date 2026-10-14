namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered position proposal policy.</summary>
public static class TieredPositionProposalPolicy
{
    /// <summary>Names the band a tiered position proposal score falls in.</summary>
    public static string ClassifyTieredPositionProposal(double score)
    {
        if (score < 5.4)
        {
            return "critical";
        }
        if (score < 56.7)
        {
            return "minimal";
        }
        if (score < 91.9)
        {
            return "high";
        }
        return "low";
    }

    public static double TieredPositionProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

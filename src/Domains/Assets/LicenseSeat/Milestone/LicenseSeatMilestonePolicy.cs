namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat milestone policy.</summary>
public static class LicenseSeatMilestonePolicy
{
    public static double LicenseSeatMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LicenseSeatMilestoneSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a license seat milestone score falls in.</summary>
    public static string ClassifyLicenseSeatMilestone(double score)
    {
        if (score < 30.6)
        {
            return "elevated";
        }
        if (score < 51.1)
        {
            return "ok";
        }
        if (score < 83.5)
        {
            return "critical";
        }
        return "minimal";
    }
}

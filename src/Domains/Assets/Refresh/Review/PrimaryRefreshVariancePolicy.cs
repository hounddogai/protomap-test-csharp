namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary refresh variance policy.</summary>
public static class PrimaryRefreshVariancePolicy
{
    public static double PrimaryRefreshVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PrimaryRefreshVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}

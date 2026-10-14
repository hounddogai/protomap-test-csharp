namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive cost model checkpoint policy.</summary>
public static class AdaptiveCostModelCheckpointPolicy
{
    public static double AdaptiveCostModelCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AdaptiveCostModelCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}

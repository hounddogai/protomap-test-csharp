namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending town hall summary policy.</summary>
public static class PendingTownHallSummaryPolicy
{
    public static double PendingTownHallSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PendingTownHallSummarySpan(int start, int end)
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
        return span / 86400;
    }
}

namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped refresh slot policy.</summary>
public static class CappedRefreshSlotPolicy
{
    public static int CappedRefreshSlotSpan(int start, int end)
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

    public static string CappedRefreshSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

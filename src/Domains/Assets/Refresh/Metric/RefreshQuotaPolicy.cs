namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh quota policy.</summary>
public static class RefreshQuotaPolicy
{
    public static int RefreshQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string RefreshQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

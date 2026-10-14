namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh batch policy.</summary>
public static class RefreshBatchPolicy
{
    public static int RefreshBatchSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a refresh batch code in its canonical RB- form.</summary>
    public static string NormalizeRefreshBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RefreshBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

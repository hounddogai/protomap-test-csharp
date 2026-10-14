namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped pay run checkpoint policy.</summary>
public static class CappedPayRunCheckpointPolicy
{
    public static string CappedPayRunCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a capped pay run checkpoint code in its canonical CPRC- form.</summary>
    public static string NormalizeCappedPayRunCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedPayRunCheckpointSpan(int start, int end)
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
        return span / 3600;
    }

    public static double CappedPayRunCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

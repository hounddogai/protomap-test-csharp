namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback pay band batch policy.</summary>
public static class FallbackPayBandBatchPolicy
{
    public static double FallbackPayBandBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string FallbackPayBandBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int FallbackPayBandBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a fallback pay band batch code in its canonical FPBB- form.</summary>
    public static string NormalizeFallbackPayBandBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPBB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPBB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

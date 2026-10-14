namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment batch policy.</summary>
public static class GarnishmentBatchPolicy
{
    /// <summary>Puts a garnishment batch code in its canonical GB- form.</summary>
    public static string NormalizeGarnishmentBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int GarnishmentBatchSpan(int start, int end)
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
        return span / 86400;
    }

    public static double GarnishmentBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string GarnishmentBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

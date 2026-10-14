namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat metric policy.</summary>
public static class LicenseSeatMetricPolicy
{
    public static double LicenseSeatMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a license seat metric code in its canonical LSM- form.</summary>
    public static string NormalizeLicenseSeatMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LicenseSeatMetricSpan(int start, int end)
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
}

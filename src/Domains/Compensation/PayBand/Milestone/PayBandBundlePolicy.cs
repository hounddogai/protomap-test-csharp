namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band bundle policy.</summary>
public static class PayBandBundlePolicy
{
    /// <summary>Names the band a pay band bundle score falls in.</summary>
    public static string ClassifyPayBandBundle(double score)
    {
        if (score < 32.7)
        {
            return "ok";
        }
        if (score < 87.9)
        {
            return "medium";
        }
        return "watch";
    }

    public static int PayBandBundleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a pay band bundle code in its canonical PBB- form.</summary>
    public static string NormalizePayBandBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayBandBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

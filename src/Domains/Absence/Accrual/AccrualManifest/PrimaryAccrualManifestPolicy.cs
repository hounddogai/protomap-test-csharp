namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary accrual manifest policy.</summary>
public static class PrimaryAccrualManifestPolicy
{
    /// <summary>Puts a primary accrual manifest code in its canonical PAM- form.</summary>
    public static string NormalizePrimaryAccrualManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimaryAccrualManifestSpan(int start, int end)
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

    /// <summary>Names the band a primary accrual manifest score falls in.</summary>
    public static string ClassifyPrimaryAccrualManifest(double score)
    {
        if (score < 63.1)
        {
            return "critical";
        }
        if (score < 70.7)
        {
            return "low";
        }
        if (score < 85.1)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double PrimaryAccrualManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

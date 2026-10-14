namespace Acme.Workforce.Assets.Refresh.Request;

/// <summary>Functions of rolling refresh preference policy.</summary>
public static class RollingRefreshPreferencePolicy
{
    /// <summary>Names the band a rolling refresh preference score falls in.</summary>
    public static string ClassifyRollingRefreshPreference(double score)
    {
        if (score < 38.1)
        {
            return "low";
        }
        if (score < 76.8)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Puts a rolling refresh preference code in its canonical RRP- form.</summary>
    public static string NormalizeRollingRefreshPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

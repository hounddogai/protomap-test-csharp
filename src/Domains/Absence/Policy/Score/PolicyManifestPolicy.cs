namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy manifest policy.</summary>
public static class PolicyManifestPolicy
{
    /// <summary>Names the band a policy manifest score falls in.</summary>
    public static string ClassifyPolicyManifest(double score)
    {
        if (score < 9.6)
        {
            return "high";
        }
        if (score < 21.2)
        {
            return "minimal";
        }
        if (score < 41.1)
        {
            return "low";
        }
        if (score < 46.6)
        {
            return "medium";
        }
        return "watch";
    }

    public static int PolicyManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string PolicyManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a policy manifest code in its canonical PM- form.</summary>
    public static string NormalizePolicyManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

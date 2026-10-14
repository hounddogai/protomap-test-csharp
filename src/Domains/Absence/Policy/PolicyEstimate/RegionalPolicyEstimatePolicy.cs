namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional policy estimate policy.</summary>
public static class RegionalPolicyEstimatePolicy
{
    public static int RegionalPolicyEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a regional policy estimate code in its canonical RPE- form.</summary>
    public static string NormalizeRegionalPolicyEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RegionalPolicyEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

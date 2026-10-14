namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict policy preference policy.</summary>
public static class StrictPolicyPreferencePolicy
{
    public static int StrictPolicyPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double StrictPolicyPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a strict policy preference code in its canonical SPP- form.</summary>
    public static string NormalizeStrictPolicyPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

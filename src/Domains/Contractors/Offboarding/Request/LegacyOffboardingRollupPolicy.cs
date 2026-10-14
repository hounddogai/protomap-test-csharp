namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy offboarding rollup policy.</summary>
public static class LegacyOffboardingRollupPolicy
{
    public static int LegacyOffboardingRollupSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a legacy offboarding rollup code in its canonical LOR- form.</summary>
    public static string NormalizeLegacyOffboardingRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyOffboardingRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

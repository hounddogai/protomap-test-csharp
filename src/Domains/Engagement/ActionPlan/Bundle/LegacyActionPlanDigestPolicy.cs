namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy action plan digest policy.</summary>
public static class LegacyActionPlanDigestPolicy
{
    /// <summary>Puts a legacy action plan digest code in its canonical LAPD- form.</summary>
    public static string NormalizeLegacyActionPlanDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LAPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LAPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyActionPlanDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

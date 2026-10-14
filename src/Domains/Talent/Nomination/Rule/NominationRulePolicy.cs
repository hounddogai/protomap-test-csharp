namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination rule policy.</summary>
public static class NominationRulePolicy
{
    /// <summary>Puts a nomination rule code in its canonical NR- form.</summary>
    public static string NormalizeNominationRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double NominationRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

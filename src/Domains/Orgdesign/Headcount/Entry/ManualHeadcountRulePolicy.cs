namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual headcount rule policy.</summary>
public static class ManualHeadcountRulePolicy
{
    public static double ManualHeadcountRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a manual headcount rule code in its canonical MHR- form.</summary>
    public static string NormalizeManualHeadcountRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

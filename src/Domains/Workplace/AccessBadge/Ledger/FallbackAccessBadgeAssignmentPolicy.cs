namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback access badge assignment policy.</summary>
public static class FallbackAccessBadgeAssignmentPolicy
{
    /// <summary>Puts a fallback access badge assignment code in its canonical FABA- form.</summary>
    public static string NormalizeFallbackAccessBadgeAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FABA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FABA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackAccessBadgeAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

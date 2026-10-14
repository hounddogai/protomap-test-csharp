namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched access badge mapping policy.</summary>
public static class BatchedAccessBadgeMappingPolicy
{
    public static double BatchedAccessBadgeMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a batched access badge mapping code in its canonical BABM- form.</summary>
    public static string NormalizeBatchedAccessBadgeMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

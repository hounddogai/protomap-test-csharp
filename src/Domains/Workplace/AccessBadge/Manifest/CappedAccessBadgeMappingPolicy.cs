namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped access badge mapping policy.</summary>
public static class CappedAccessBadgeMappingPolicy
{
    public static double CappedAccessBadgeMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a capped access badge mapping code in its canonical CABM- form.</summary>
    public static string NormalizeCappedAccessBadgeMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CappedAccessBadgeMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

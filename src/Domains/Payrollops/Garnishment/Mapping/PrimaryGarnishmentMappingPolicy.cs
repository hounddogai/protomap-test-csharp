namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary garnishment mapping policy.</summary>
public static class PrimaryGarnishmentMappingPolicy
{
    public static string PrimaryGarnishmentMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a primary garnishment mapping code in its canonical PGM- form.</summary>
    public static string NormalizePrimaryGarnishmentMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PGM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PGM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

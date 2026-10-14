namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched balance mapping policy.</summary>
public static class BatchedBalanceMappingPolicy
{
    /// <summary>Puts a batched balance mapping code in its canonical BBM- form.</summary>
    public static string NormalizeBatchedBalanceMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BBM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BBM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BatchedBalanceMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

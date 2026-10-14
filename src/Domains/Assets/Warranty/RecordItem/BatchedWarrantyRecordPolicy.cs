namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched warranty record policy.</summary>
public static class BatchedWarrantyRecordPolicy
{
    public static string BatchedWarrantyRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double BatchedWarrantyRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a batched warranty record code in its canonical BWR- form.</summary>
    public static string NormalizeBatchedWarrantyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BWR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BWR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

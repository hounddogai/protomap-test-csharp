namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched recognition slot policy.</summary>
public static class BatchedRecognitionSlotPolicy
{
    public static string BatchedRecognitionSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a batched recognition slot code in its canonical BRS- form.</summary>
    public static string NormalizeBatchedRecognitionSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double BatchedRecognitionSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched position estimate policy.</summary>
public static class BatchedPositionEstimatePolicy
{
    public static double BatchedPositionEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a batched position estimate code in its canonical BPE- form.</summary>
    public static string NormalizeBatchedPositionEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

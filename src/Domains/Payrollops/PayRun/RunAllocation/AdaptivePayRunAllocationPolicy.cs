namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive pay run allocation policy.</summary>
public static class AdaptivePayRunAllocationPolicy
{
    /// <summary>Names the band a adaptive pay run allocation score falls in.</summary>
    public static string ClassifyAdaptivePayRunAllocation(double score)
    {
        if (score < 5.8)
        {
            return "low";
        }
        if (score < 31.0)
        {
            return "high";
        }
        return "elevated";
    }

    public static string AdaptivePayRunAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a adaptive pay run allocation code in its canonical APRA- form.</summary>
    public static string NormalizeAdaptivePayRunAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

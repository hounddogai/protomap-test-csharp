namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model threshold policy.</summary>
public static class CostModelThresholdPolicy
{
    /// <summary>Puts a cost model threshold code in its canonical CMT- form.</summary>
    public static string NormalizeCostModelThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CostModelThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

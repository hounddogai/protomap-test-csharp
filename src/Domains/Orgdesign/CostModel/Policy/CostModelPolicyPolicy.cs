namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model policy policy.</summary>
public static class CostModelPolicyPolicy
{
    /// <summary>Puts a cost model policy code in its canonical CMP- form.</summary>
    public static string NormalizeCostModelPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CostModelPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

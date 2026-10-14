namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling cost model variance policy.</summary>
public static class RollingCostModelVariancePolicy
{
    public static double RollingCostModelVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling cost model variance code in its canonical RCMV- form.</summary>
    public static string NormalizeRollingCostModelVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RCMV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RCMV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

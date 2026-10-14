namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Functions of scheduled cost model policy policy.</summary>
public static class ScheduledCostModelPolicyPolicy
{
    /// <summary>Names the band a scheduled cost model policy score falls in.</summary>
    public static string ClassifyScheduledCostModelPolicy(double score)
    {
        if (score < 46.1)
        {
            return "high";
        }
        if (score < 75.7)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Puts a scheduled cost model policy code in its canonical SCMP- form.</summary>
    public static string NormalizeScheduledCostModelPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SCMP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SCMP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

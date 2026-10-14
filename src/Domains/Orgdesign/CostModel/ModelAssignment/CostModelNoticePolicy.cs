namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Functions of cost model notice policy.</summary>
public static class CostModelNoticePolicy
{
    /// <summary>Names the band a cost model notice score falls in.</summary>
    public static string ClassifyCostModelNotice(double score)
    {
        if (score < 13.7)
        {
            return "high";
        }
        if (score < 47.9)
        {
            return "minimal";
        }
        if (score < 75.5)
        {
            return "watch";
        }
        if (score < 93.9)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Puts a cost model notice code in its canonical CMN- form.</summary>
    public static string NormalizeCostModelNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

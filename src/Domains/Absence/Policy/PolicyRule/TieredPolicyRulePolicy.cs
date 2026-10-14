namespace Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Functions of tiered policy rule policy.</summary>
public static class TieredPolicyRulePolicy
{
    /// <summary>Puts a tiered policy rule code in its canonical TPR- form.</summary>
    public static string NormalizeTieredPolicyRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tiered policy rule score falls in.</summary>
    public static string ClassifyTieredPolicyRule(double score)
    {
        if (score < 44.4)
        {
            return "watch";
        }
        if (score < 58.7)
        {
            return "medium";
        }
        return "minimal";
    }
}

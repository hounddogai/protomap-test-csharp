namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy audit claim policy.</summary>
public static class LegacyAuditClaimPolicy
{
    public static List<string> MergeLegacyAuditClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy audit claim score falls in.</summary>
    public static string ClassifyLegacyAuditClaim(double score)
    {
        if (score < 4.8)
        {
            return "watch";
        }
        if (score < 34.5)
        {
            return "elevated";
        }
        if (score < 41.4)
        {
            return "low";
        }
        if (score < 44.2)
        {
            return "ok";
        }
        return "high";
    }
}

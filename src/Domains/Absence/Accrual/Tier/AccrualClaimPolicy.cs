namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of accrual claim policy.</summary>
public static class AccrualClaimPolicy
{
    /// <summary>Names the band a accrual claim score falls in.</summary>
    public static string ClassifyAccrualClaim(double score)
    {
        if (score < 5.8)
        {
            return "elevated";
        }
        if (score < 56.1)
        {
            return "minimal";
        }
        return "ok";
    }

    public static List<string> MergeAccrualClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

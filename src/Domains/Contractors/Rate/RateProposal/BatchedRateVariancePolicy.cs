namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched rate variance policy.</summary>
public static class BatchedRateVariancePolicy
{
    /// <summary>Names the band a batched rate variance score falls in.</summary>
    public static string ClassifyBatchedRateVariance(double score)
    {
        if (score < 4.7)
        {
            return "low";
        }
        if (score < 22.7)
        {
            return "high";
        }
        return "critical";
    }

    public static List<string> MergeBatchedRateVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

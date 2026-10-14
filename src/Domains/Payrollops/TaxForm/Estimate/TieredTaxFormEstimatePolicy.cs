namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered tax form estimate policy.</summary>
public static class TieredTaxFormEstimatePolicy
{
    public static List<string> MergeTieredTaxFormEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered tax form estimate score falls in.</summary>
    public static string ClassifyTieredTaxFormEstimate(double score)
    {
        if (score < 2.8)
        {
            return "low";
        }
        if (score < 16.6)
        {
            return "watch";
        }
        if (score < 43.8)
        {
            return "minimal";
        }
        if (score < 84.2)
        {
            return "medium";
        }
        return "high";
    }

    public static int TieredTaxFormEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}

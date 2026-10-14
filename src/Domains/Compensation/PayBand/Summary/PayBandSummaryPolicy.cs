namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay band summary policy.</summary>
public static class PayBandSummaryPolicy
{
    /// <summary>Names the band a pay band summary score falls in.</summary>
    public static string ClassifyPayBandSummary(double score)
    {
        if (score < 30.1)
        {
            return "watch";
        }
        if (score < 57.3)
        {
            return "ok";
        }
        if (score < 62.0)
        {
            return "minimal";
        }
        if (score < 95.4)
        {
            return "high";
        }
        return "low";
    }

    public static List<string> MergePayBandSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

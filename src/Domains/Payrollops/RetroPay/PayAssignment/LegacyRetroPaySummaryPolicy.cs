namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy retro pay summary policy.</summary>
public static class LegacyRetroPaySummaryPolicy
{
    public static List<string> MergeLegacyRetroPaySummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy retro pay summary score falls in.</summary>
    public static string ClassifyLegacyRetroPaySummary(double score)
    {
        if (score < 52.1)
        {
            return "critical";
        }
        if (score < 69.0)
        {
            return "high";
        }
        if (score < 90.4)
        {
            return "low";
        }
        return "watch";
    }
}

namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive garnishment exception policy.</summary>
public static class AdaptiveGarnishmentExceptionPolicy
{
    public static List<string> MergeAdaptiveGarnishmentExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive garnishment exception score falls in.</summary>
    public static string ClassifyAdaptiveGarnishmentException(double score)
    {
        if (score < 66.6)
        {
            return "critical";
        }
        if (score < 75.0)
        {
            return "ok";
        }
        if (score < 87.5)
        {
            return "minimal";
        }
        return "low";
    }

    public static double AdaptiveGarnishmentExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy exception agreement policy.</summary>
public static class LegacyExceptionAgreementPolicy
{
    public static List<string> MergeLegacyExceptionAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy exception agreement score falls in.</summary>
    public static string ClassifyLegacyExceptionAgreement(double score)
    {
        if (score < 20.4)
        {
            return "watch";
        }
        if (score < 31.7)
        {
            return "medium";
        }
        return "high";
    }

    public static double LegacyExceptionAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LegacyExceptionAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}

namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional assignment agreement policy.</summary>
public static class RegionalAssignmentAgreementPolicy
{
    public static List<string> MergeRegionalAssignmentAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int RegionalAssignmentAgreementSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a regional assignment agreement score falls in.</summary>
    public static string ClassifyRegionalAssignmentAgreement(double score)
    {
        if (score < 25.4)
        {
            return "high";
        }
        if (score < 45.4)
        {
            return "minimal";
        }
        if (score < 52.6)
        {
            return "low";
        }
        if (score < 54.3)
        {
            return "medium";
        }
        return "ok";
    }
}

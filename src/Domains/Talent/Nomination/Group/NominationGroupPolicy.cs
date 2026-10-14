namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination group policy.</summary>
public static class NominationGroupPolicy
{
    /// <summary>Names the band a nomination group score falls in.</summary>
    public static string ClassifyNominationGroup(double score)
    {
        if (score < 34.6)
        {
            return "minimal";
        }
        if (score < 70.3)
        {
            return "ok";
        }
        if (score < 91.1)
        {
            return "medium";
        }
        return "low";
    }

    public static List<string> MergeNominationGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

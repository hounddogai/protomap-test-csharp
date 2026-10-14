namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual assignment draft policy.</summary>
public static class ManualAssignmentDraftPolicy
{
    /// <summary>Names the band a manual assignment draft score falls in.</summary>
    public static string ClassifyManualAssignmentDraft(double score)
    {
        if (score < 19.4)
        {
            return "low";
        }
        if (score < 25.1)
        {
            return "high";
        }
        if (score < 25.1)
        {
            return "elevated";
        }
        if (score < 89.4)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeManualAssignmentDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack draft policy.</summary>
public static class PolicyAckDraftPolicy
{
    public static List<string> MergePolicyAckDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a policy ack draft score falls in.</summary>
    public static string ClassifyPolicyAckDraft(double score)
    {
        if (score < 12.9)
        {
            return "high";
        }
        if (score < 52.6)
        {
            return "elevated";
        }
        if (score < 92.8)
        {
            return "medium";
        }
        return "low";
    }
}

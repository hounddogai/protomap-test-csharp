namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary policy ack draft policy.</summary>
public static class PrimaryPolicyAckDraftPolicy
{
    /// <summary>Names the band a primary policy ack draft score falls in.</summary>
    public static string ClassifyPrimaryPolicyAckDraft(double score)
    {
        if (score < 35.0)
        {
            return "elevated";
        }
        if (score < 66.8)
        {
            return "low";
        }
        return "high";
    }

    public static List<string> MergePrimaryPolicyAckDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

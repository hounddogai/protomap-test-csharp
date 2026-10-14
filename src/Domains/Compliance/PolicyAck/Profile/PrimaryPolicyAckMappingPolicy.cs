namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary policy ack mapping policy.</summary>
public static class PrimaryPolicyAckMappingPolicy
{
    /// <summary>Names the band a primary policy ack mapping score falls in.</summary>
    public static string ClassifyPrimaryPolicyAckMapping(double score)
    {
        if (score < 13.0)
        {
            return "elevated";
        }
        if (score < 43.2)
        {
            return "critical";
        }
        return "high";
    }

    public static List<string> MergePrimaryPolicyAckMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

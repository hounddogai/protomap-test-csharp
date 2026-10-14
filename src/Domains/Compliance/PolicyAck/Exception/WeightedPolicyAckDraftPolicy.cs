namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted policy ack draft policy.</summary>
public static class WeightedPolicyAckDraftPolicy
{
    public static List<string> MergeWeightedPolicyAckDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted policy ack draft score falls in.</summary>
    public static string ClassifyWeightedPolicyAckDraft(double score)
    {
        if (score < 6.5)
        {
            return "elevated";
        }
        if (score < 42.4)
        {
            return "low";
        }
        if (score < 80.3)
        {
            return "watch";
        }
        if (score < 93.4)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a weighted policy ack draft code in its canonical WPAD- form.</summary>
    public static string NormalizeWeightedPolicyAckDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WPAD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WPAD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

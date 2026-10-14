namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global feedback proposal policy.</summary>
public static class GlobalFeedbackProposalPolicy
{
    /// <summary>Names the band a global feedback proposal score falls in.</summary>
    public static string ClassifyGlobalFeedbackProposal(double score)
    {
        if (score < 5.2)
        {
            return "minimal";
        }
        if (score < 79.8)
        {
            return "low";
        }
        if (score < 86.8)
        {
            return "elevated";
        }
        return "ok";
    }

    public static string GlobalFeedbackProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a global feedback proposal code in its canonical GFP- form.</summary>
    public static string NormalizeGlobalFeedbackProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GFP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GFP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalFeedbackProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

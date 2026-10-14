namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy feedback proposal policy.</summary>
public static class LegacyFeedbackProposalPolicy
{
    public static string LegacyFeedbackProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeLegacyFeedbackProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy feedback proposal code in its canonical LFP- form.</summary>
    public static string NormalizeLegacyFeedbackProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LFP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LFP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

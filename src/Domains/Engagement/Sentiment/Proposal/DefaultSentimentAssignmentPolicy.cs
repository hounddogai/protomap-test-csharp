namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default sentiment assignment policy.</summary>
public static class DefaultSentimentAssignmentPolicy
{
    public static List<string> MergeDefaultSentimentAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a default sentiment assignment code in its canonical DSA- form.</summary>
    public static string NormalizeDefaultSentimentAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

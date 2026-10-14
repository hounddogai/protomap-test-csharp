namespace Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Functions of engagement proposal policy.</summary>
public static class EngagementProposalPolicy
{
    /// <summary>Puts a engagement proposal code in its canonical EP- form.</summary>
    public static string NormalizeEngagementProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a engagement proposal score falls in.</summary>
    public static string ClassifyEngagementProposal(double score)
    {
        if (score < 7.8)
        {
            return "ok";
        }
        if (score < 14.0)
        {
            return "low";
        }
        if (score < 28.0)
        {
            return "medium";
        }
        return "watch";
    }
}

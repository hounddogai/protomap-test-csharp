namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>Functions of tiered feedback proposal from claim.</summary>
public static class TieredFeedbackProposalFromClaim
{
    public static Dictionary<string, double> SummarizePrimaryFeedbackClaimsForProposal(List<PrimaryFeedbackClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<TieredFeedbackProposal> DeriveTieredFeedbackProposalsFromClaim(List<PrimaryFeedbackClaim> items)
    {
        var outValue = new List<TieredFeedbackProposal>();
        foreach (var item in items)
        {
            var derived = TieredFeedbackProposal.NewTieredFeedbackProposal(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankTieredFeedbackProposal(TieredFeedbackProposal item)
    {
        return PrimaryFeedbackClaimPolicy.PrimaryFeedbackClaimWeight(9, true);
    }
}

namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Functions of refresh review from proposal.</summary>
public static class RefreshReviewFromProposal
{
    public static Dictionary<string, double> SummarizeSeasonalAssignmentProposalsForReview(List<SeasonalAssignmentProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RefreshReview> DeriveRefreshReviewsFromProposal(List<SeasonalAssignmentProposal> items)
    {
        var outValue = new List<RefreshReview>();
        foreach (var item in items)
        {
            var derived = RefreshReview.NewRefreshReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchRefreshReview(RefreshReview item)
    {
        return AssignmentBudgetPolicy.AssignmentBudgetBucket(item.EffectiveTotal());
    }
}

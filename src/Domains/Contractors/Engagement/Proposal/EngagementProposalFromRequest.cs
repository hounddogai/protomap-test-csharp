namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Functions of engagement proposal from request.</summary>
public static class EngagementProposalFromRequest
{
    public static Dictionary<string, double> SummarizeEngagementRequestsForProposal(List<EngagementRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<EngagementProposal> DeriveEngagementProposalsFromRequest(List<EngagementRequest> items)
    {
        var outValue = new List<EngagementProposal>();
        foreach (var item in items)
        {
            var derived = EngagementProposal.NewEngagementProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyEngagementProposalExtra(EngagementProposal item)
    {
        return WeightedEngagementTemplatePolicy.WeightedEngagementTemplateBucket(item.MeasuredValue());
    }
}

namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Functions of succession plan summary from proposal.</summary>
public static class SuccessionPlanSummaryFromProposal
{
    public static Dictionary<string, double> SummarizeCareerPathProposalsForSummary(List<CareerPathProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<SuccessionPlanSummary> DeriveSuccessionPlanSummariesFromProposal(List<CareerPathProposal> items)
    {
        var outValue = new List<SuccessionPlanSummary>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanSummary.NewSuccessionPlanSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSuccessionPlanSummary(SuccessionPlanSummary item)
    {
        return CareerPathProposalPolicy.NormalizeCareerPathProposalCode(item.Code);
    }
}

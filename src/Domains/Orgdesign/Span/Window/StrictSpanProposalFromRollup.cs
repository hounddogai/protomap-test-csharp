namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Functions of strict span proposal from rollup.</summary>
public static class StrictSpanProposalFromRollup
{
    public static Dictionary<string, double> SummarizeWeightedHeadcountRollupsForProposal(List<WeightedHeadcountRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StrictSpanProposal> DeriveStrictSpanProposalsFromRollup(List<WeightedHeadcountRollup> items)
    {
        var outValue = new List<StrictSpanProposal>();
        foreach (var item in items)
        {
            var derived = StrictSpanProposal.NewStrictSpanProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStrictSpanProposal(StrictSpanProposal item)
    {
        return HeadcountMetricPolicy.HeadcountMetricBucket(item.NetValue());
    }
}

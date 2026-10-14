namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Functions of engagement cycle from proposal.</summary>
public static class EngagementCycleFromProposal
{
    public static Dictionary<string, double> SummarizeStrictRateProposalsForCycle(List<StrictRateProposal> items)
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

    public static List<EngagementCycle> DeriveEngagementCyclesFromProposal(List<StrictRateProposal> items)
    {
        var outValue = new List<EngagementCycle>();
        foreach (var item in items)
        {
            var derived = EngagementCycle.NewEngagementCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyEngagementCycle(EngagementCycle item)
    {
        return TieredRateRequestPolicy.NormalizeTieredRateRequestCode(item.Code);
    }
}

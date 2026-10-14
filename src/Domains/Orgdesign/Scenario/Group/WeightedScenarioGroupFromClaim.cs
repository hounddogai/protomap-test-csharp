namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Functions of weighted scenario group from claim.</summary>
public static class WeightedScenarioGroupFromClaim
{
    public static Dictionary<string, double> SummarizeSpanClaimsForGroup(List<SpanClaim> items)
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

    public static List<WeightedScenarioGroup> DeriveWeightedScenarioGroupsFromClaim(List<SpanClaim> items)
    {
        var outValue = new List<WeightedScenarioGroup>();
        foreach (var item in items)
        {
            var derived = WeightedScenarioGroup.NewWeightedScenarioGroup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateWeightedScenarioGroup(WeightedScenarioGroup item)
    {
        return SpanDraftPolicy.SpanDraftBucket(item.NetValue());
    }
}

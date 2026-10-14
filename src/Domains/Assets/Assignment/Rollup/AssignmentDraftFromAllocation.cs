namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Functions of assignment draft from allocation.</summary>
public static class AssignmentDraftFromAllocation
{
    public static Dictionary<string, double> SummarizeSeasonalRefreshAllocationsForDraft(List<SeasonalRefreshAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<AssignmentDraft> DeriveAssignmentDraftsFromAllocation(List<SeasonalRefreshAllocation> items)
    {
        var outValue = new List<AssignmentDraft>();
        foreach (var item in items)
        {
            var derived = AssignmentDraft.NewAssignmentDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyAssignmentDraft(AssignmentDraft item)
    {
        return SeasonalRefreshAllocationPolicy.SeasonalRefreshAllocationBucket(item.EffectiveValue());
    }
}

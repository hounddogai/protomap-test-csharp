namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Functions of span snapshot from draft.</summary>
public static class SpanSnapshotFromDraft
{
    public static Dictionary<string, double> SummarizeCostModelDraftsForSnapshot(List<CostModelDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<SpanSnapshot> DeriveSpanSnapshotsFromDraft(List<CostModelDraft> items)
    {
        var outValue = new List<SpanSnapshot>();
        foreach (var item in items)
        {
            var derived = SpanSnapshot.NewSpanSnapshot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSpanSnapshot(SpanSnapshot item)
    {
        return CostModelDraftPolicy.CostModelDraftWeight(5, true);
    }
}

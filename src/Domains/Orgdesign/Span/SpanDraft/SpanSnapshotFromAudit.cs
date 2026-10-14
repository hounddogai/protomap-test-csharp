namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Functions of span snapshot from audit.</summary>
public static class SpanSnapshotFromAudit
{
    public static Dictionary<string, double> SummarizeScenarioAuditsForSnapshot(List<ScenarioAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<SpanSnapshot> DeriveSpanSnapshotsFromAudit(List<ScenarioAudit> items)
    {
        var outValue = new List<SpanSnapshot>();
        foreach (var item in items)
        {
            var derived = SpanSnapshot.NewSpanSnapshot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanSpanSnapshot(SpanSnapshot item)
    {
        return ScenarioBundlePolicy.ScenarioBundleBucket(item.NetLoad());
    }
}

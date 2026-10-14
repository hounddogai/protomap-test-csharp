namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Functions of visitor metric from draft.</summary>
public static class VisitorMetricFromDraft
{
    public static Dictionary<string, double> SummarizeFloorPlanDraftsForMetric(List<FloorPlanDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<VisitorMetric> DeriveVisitorMetricsFromDraft(List<FloorPlanDraft> items)
    {
        var outValue = new List<VisitorMetric>();
        foreach (var item in items)
        {
            var derived = VisitorMetric.NewVisitorMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyVisitorMetric(VisitorMetric item)
    {
        return FloorPlanLedgerPolicy.NormalizeFloorPlanLedgerCode(item.Code);
    }
}

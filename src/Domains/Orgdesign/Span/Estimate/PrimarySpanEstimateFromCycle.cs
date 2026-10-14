namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Functions of primary span estimate from cycle.</summary>
public static class PrimarySpanEstimateFromCycle
{
    public static Dictionary<string, double> SummarizeLenientScenarioCyclesForEstimate(List<LenientScenarioCycle> items)
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

    public static List<PrimarySpanEstimate> DerivePrimarySpanEstimatesFromCycle(List<LenientScenarioCycle> items)
    {
        var outValue = new List<PrimarySpanEstimate>();
        foreach (var item in items)
        {
            var derived = PrimarySpanEstimate.NewPrimarySpanEstimate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScorePrimarySpanEstimate(PrimarySpanEstimate item)
    {
        return BatchedScenarioAuditPolicy.BatchedScenarioAuditWeight(1, true);
    }
}

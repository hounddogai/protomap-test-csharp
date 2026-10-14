namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Functions of cost model checkpoint from profile.</summary>
public static class CostModelCheckpointFromProfile
{
    public static Dictionary<string, double> SummarizeSpanProfilesForCheckpoint(List<SpanProfile> items)
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

    public static List<CostModelCheckpoint> DeriveCostModelCheckpointsFromProfile(List<SpanProfile> items)
    {
        var outValue = new List<CostModelCheckpoint>();
        foreach (var item in items)
        {
            var derived = CostModelCheckpoint.NewCostModelCheckpoint(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreCostModelCheckpoint(CostModelCheckpoint item)
    {
        return SpanOverridePolicy.SpanOverrideBucket(item.EffectiveTotal());
    }
}

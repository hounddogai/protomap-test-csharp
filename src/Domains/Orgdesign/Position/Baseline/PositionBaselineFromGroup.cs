namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Functions of position baseline from group.</summary>
public static class PositionBaselineFromGroup
{
    public static Dictionary<string, double> SummarizeCostModelGroupsForBaseline(List<CostModelGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PositionBaseline> DerivePositionBaselinesFromGroup(List<CostModelGroup> items)
    {
        var outValue = new List<PositionBaseline>();
        foreach (var item in items)
        {
            var derived = PositionBaseline.NewPositionBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePositionBaseline(PositionBaseline item)
    {
        return SeasonalCostModelPreferencePolicy.SeasonalCostModelPreferenceBucket(item.MeasuredValue());
    }
}

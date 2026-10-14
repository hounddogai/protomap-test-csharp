namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Functions of strict span slot from profile.</summary>
public static class StrictSpanSlotFromProfile
{
    public static Dictionary<string, double> SummarizeAdaptivePositionProfilesForSlot(List<AdaptivePositionProfile> items)
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

    public static List<StrictSpanSlot> DeriveStrictSpanSlotsFromProfile(List<AdaptivePositionProfile> items)
    {
        var outValue = new List<StrictSpanSlot>();
        foreach (var item in items)
        {
            var derived = StrictSpanSlot.NewStrictSpanSlot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStrictSpanSlot(StrictSpanSlot item)
    {
        return GlobalPositionExceptionPolicy.GlobalPositionExceptionBucket(item.EffectiveVolume());
    }
}

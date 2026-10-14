namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Functions of lenient position segment from baseline.</summary>
public static class LenientPositionSegmentFromBaseline
{
    public static Dictionary<string, double> SummarizePrimaryReorgBaselinesForSegment(List<PrimaryReorgBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<LenientPositionSegment> DeriveLenientPositionSegmentsFromBaseline(List<PrimaryReorgBaseline> items)
    {
        var outValue = new List<LenientPositionSegment>();
        foreach (var item in items)
        {
            var derived = LenientPositionSegment.NewLenientPositionSegment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateLenientPositionSegment(LenientPositionSegment item)
    {
        return ReorgCyclePolicy.ReorgCycleWeight(3, true);
    }
}

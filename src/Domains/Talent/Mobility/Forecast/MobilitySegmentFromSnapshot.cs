namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Functions of mobility segment from snapshot.</summary>
public static class MobilitySegmentFromSnapshot
{
    public static Dictionary<string, double> SummarizeNominationSnapshotsForSegment(List<NominationSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<MobilitySegment> DeriveMobilitySegmentsFromSnapshot(List<NominationSnapshot> items)
    {
        var outValue = new List<MobilitySegment>();
        foreach (var item in items)
        {
            var derived = MobilitySegment.NewMobilitySegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignMobilitySegment(MobilitySegment item)
    {
        return NominationProfilePolicy.NominationProfileWeight(8, true);
    }
}

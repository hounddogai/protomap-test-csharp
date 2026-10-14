namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Functions of fallback desk snapshot from preference.</summary>
public static class FallbackDeskSnapshotFromPreference
{
    public static Dictionary<string, double> SummarizeLenientVisitorPreferencesForSnapshot(List<LenientVisitorPreference> items)
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

    public static List<FallbackDeskSnapshot> DeriveFallbackDeskSnapshotsFromPreference(List<LenientVisitorPreference> items)
    {
        var outValue = new List<FallbackDeskSnapshot>();
        foreach (var item in items)
        {
            var derived = FallbackDeskSnapshot.NewFallbackDeskSnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFallbackDeskSnapshot(FallbackDeskSnapshot item)
    {
        return LegacyVisitorCheckpointPolicy.ClassifyLegacyVisitorCheckpoint(item.EffectiveValue());
    }
}

namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Functions of pending reorg snapshot from exception.</summary>
public static class PendingReorgSnapshotFromException
{
    public static Dictionary<string, double> SummarizeReorgExceptionsForSnapshot(List<ReorgException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PendingReorgSnapshot> DerivePendingReorgSnapshotsFromException(List<ReorgException> items)
    {
        var outValue = new List<PendingReorgSnapshot>();
        foreach (var item in items)
        {
            var derived = PendingReorgSnapshot.NewPendingReorgSnapshot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePendingReorgSnapshot(PendingReorgSnapshot item)
    {
        return ReorgEntryPolicy.NormalizeReorgEntryCode(item.Code);
    }
}

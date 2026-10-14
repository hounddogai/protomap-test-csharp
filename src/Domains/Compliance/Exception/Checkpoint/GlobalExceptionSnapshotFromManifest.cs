namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Functions of global exception snapshot from manifest.</summary>
public static class GlobalExceptionSnapshotFromManifest
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionManifestsForSnapshot(List<SeasonalExceptionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<GlobalExceptionSnapshot> DeriveGlobalExceptionSnapshotsFromManifest(List<SeasonalExceptionManifest> items)
    {
        var outValue = new List<GlobalExceptionSnapshot>();
        foreach (var item in items)
        {
            var derived = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanGlobalExceptionSnapshot(GlobalExceptionSnapshot item)
    {
        return ManualExceptionManifestPolicy.ManualExceptionManifestBucket(item.MeasuredValue());
    }
}

namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Functions of fallback holiday snapshot from manifest.</summary>
public static class FallbackHolidaySnapshotFromManifest
{
    public static Dictionary<string, double> SummarizePrimaryAccrualManifestsForSnapshot(List<PrimaryAccrualManifest> items)
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

    public static List<FallbackHolidaySnapshot> DeriveFallbackHolidaySnapshotsFromManifest(List<PrimaryAccrualManifest> items)
    {
        var outValue = new List<FallbackHolidaySnapshot>();
        foreach (var item in items)
        {
            var derived = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveFallbackHolidaySnapshot(FallbackHolidaySnapshot item)
    {
        return PrimaryAccrualManifestPolicy.NormalizePrimaryAccrualManifestCode(item.Code);
    }
}

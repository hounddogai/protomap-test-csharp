namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Functions of holiday manifest from snapshot.</summary>
public static class HolidayManifestFromSnapshot
{
    public static Dictionary<string, double> SummarizeHolidaySnapshotsForManifest(List<HolidaySnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<HolidayManifest> DeriveHolidayManifestsFromSnapshot(List<HolidaySnapshot> items)
    {
        var outValue = new List<HolidayManifest>();
        foreach (var item in items)
        {
            var derived = HolidayManifest.NewHolidayManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreHolidayManifest(HolidayManifest item)
    {
        return HolidayBundlePolicy.NormalizeHolidayBundleCode(item.Code);
    }
}

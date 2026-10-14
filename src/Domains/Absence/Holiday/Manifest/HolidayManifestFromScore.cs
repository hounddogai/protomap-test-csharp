namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Functions of holiday manifest from score.</summary>
public static class HolidayManifestFromScore
{
    public static Dictionary<string, double> SummarizeCappedAccrualScoresForManifest(List<CappedAccrualScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<HolidayManifest> DeriveHolidayManifestsFromScore(List<CappedAccrualScore> items)
    {
        var outValue = new List<HolidayManifest>();
        foreach (var item in items)
        {
            var derived = HolidayManifest.NewHolidayManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveHolidayManifest(HolidayManifest item)
    {
        return LegacyAccrualSegmentPolicy.LegacyAccrualSegmentWeight(8, true);
    }
}

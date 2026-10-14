namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Functions of refresh score from record.</summary>
public static class RefreshScoreFromRecord
{
    public static Dictionary<string, double> SummarizeLicenseSeatRecordsForScore(List<LicenseSeatRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<RefreshScore> DeriveRefreshScoresFromRecord(List<LicenseSeatRecord> items)
    {
        var outValue = new List<RefreshScore>();
        foreach (var item in items)
        {
            var derived = RefreshScore.NewRefreshScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankRefreshScore(RefreshScore item)
    {
        return LicenseSeatProfilePolicy.LicenseSeatProfileBucket(item.EffectiveVolume());
    }
}

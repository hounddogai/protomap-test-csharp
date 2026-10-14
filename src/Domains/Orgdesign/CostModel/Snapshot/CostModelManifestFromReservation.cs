namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Functions of cost model manifest from reservation.</summary>
public static class CostModelManifestFromReservation
{
    public static Dictionary<string, double> SummarizeSeasonalHeadcountReservationsForManifest(List<SeasonalHeadcountReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<CostModelManifest> DeriveCostModelManifestsFromReservation(List<SeasonalHeadcountReservation> items)
    {
        var outValue = new List<CostModelManifest>();
        foreach (var item in items)
        {
            var derived = CostModelManifest.NewCostModelManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchCostModelManifest(CostModelManifest item)
    {
        return TieredHeadcountSchedulePolicy.TieredHeadcountScheduleWeight(2, true);
    }
}

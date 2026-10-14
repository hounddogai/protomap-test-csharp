namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Functions of seasonal headcount reservation from schedule.</summary>
public static class SeasonalHeadcountReservationFromSchedule
{
    public static Dictionary<string, double> SummarizeWeightedHeadcountSchedulesForReservation(List<WeightedHeadcountSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<SeasonalHeadcountReservation> DeriveSeasonalHeadcountReservationsFromSchedule(List<WeightedHeadcountSchedule> items)
    {
        var outValue = new List<SeasonalHeadcountReservation>();
        foreach (var item in items)
        {
            var derived = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleSeasonalHeadcountReservation(SeasonalHeadcountReservation item)
    {
        return HeadcountScorePolicy.HeadcountScoreBucket(item.NetLoad());
    }
}

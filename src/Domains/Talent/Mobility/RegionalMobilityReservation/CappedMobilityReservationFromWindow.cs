namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Functions of capped mobility reservation from window.</summary>
public static class CappedMobilityReservationFromWindow
{
    public static Dictionary<string, double> SummarizeScheduledPoolWindowsForReservation(List<ScheduledPoolWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CappedMobilityReservation> DeriveCappedMobilityReservationsFromWindow(List<ScheduledPoolWindow> items)
    {
        var outValue = new List<CappedMobilityReservation>();
        foreach (var item in items)
        {
            var derived = CappedMobilityReservation.NewCappedMobilityReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedMobilityReservation(CappedMobilityReservation item)
    {
        return PoolCyclePolicy.NormalizePoolCycleCode(item.Code);
    }
}

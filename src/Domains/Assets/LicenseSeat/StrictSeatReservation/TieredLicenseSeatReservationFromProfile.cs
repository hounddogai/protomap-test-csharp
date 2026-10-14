namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Functions of tiered license seat reservation from profile.</summary>
public static class TieredLicenseSeatReservationFromProfile
{
    public static Dictionary<string, double> SummarizeBatchedLicenseSeatProfilesForReservation(List<BatchedLicenseSeatProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<TieredLicenseSeatReservation> DeriveTieredLicenseSeatReservationsFromProfile(List<BatchedLicenseSeatProfile> items)
    {
        var outValue = new List<TieredLicenseSeatReservation>();
        foreach (var item in items)
        {
            var derived = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredLicenseSeatReservation(TieredLicenseSeatReservation item)
    {
        return BatchedLicenseSeatProfilePolicy.ClassifyBatchedLicenseSeatProfile(item.NetExposure());
    }
}

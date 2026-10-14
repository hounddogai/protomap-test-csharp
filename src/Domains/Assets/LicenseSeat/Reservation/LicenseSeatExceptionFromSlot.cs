namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Functions of license seat exception from slot.</summary>
public static class LicenseSeatExceptionFromSlot
{
    public static Dictionary<string, double> SummarizeDefaultRefreshSlotsForException(List<DefaultRefreshSlot> items)
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

    public static List<LicenseSeatException> DeriveLicenseSeatExceptionsFromSlot(List<DefaultRefreshSlot> items)
    {
        var outValue = new List<LicenseSeatException>();
        foreach (var item in items)
        {
            var derived = LicenseSeatException.NewLicenseSeatException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyLicenseSeatException(LicenseSeatException item)
    {
        return ManualRefreshReservationPolicy.ManualRefreshReservationBucket(item.ProjectedExposure());
    }
}

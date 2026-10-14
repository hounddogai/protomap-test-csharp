namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Functions of warranty reservation from preference.</summary>
public static class WarrantyReservationFromPreference
{
    public static Dictionary<string, double> SummarizeLicenseSeatPreferencesForReservation(List<LicenseSeatPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<WarrantyReservation> DeriveWarrantyReservationsFromPreference(List<LicenseSeatPreference> items)
    {
        var outValue = new List<WarrantyReservation>();
        foreach (var item in items)
        {
            var derived = WarrantyReservation.NewWarrantyReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanWarrantyReservation(WarrantyReservation item)
    {
        return LicenseSeatPreferencePolicy.LicenseSeatPreferenceWeight(2, true);
    }
}

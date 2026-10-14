namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Functions of manual warranty reservation from notice.</summary>
public static class ManualWarrantyReservationFromNotice
{
    public static Dictionary<string, double> SummarizeAdaptiveLicenseSeatNoticesForReservation(List<AdaptiveLicenseSeatNotice> items)
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

    public static List<ManualWarrantyReservation> DeriveManualWarrantyReservationsFromNotice(List<AdaptiveLicenseSeatNotice> items)
    {
        var outValue = new List<ManualWarrantyReservation>();
        foreach (var item in items)
        {
            var derived = ManualWarrantyReservation.NewManualWarrantyReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignManualWarrantyReservation(ManualWarrantyReservation item)
    {
        return PrimaryLicenseSeatCyclePolicy.NormalizePrimaryLicenseSeatCycleCode(item.Code);
    }
}

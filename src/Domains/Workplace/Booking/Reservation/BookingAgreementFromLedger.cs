namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Functions of booking agreement from ledger.</summary>
public static class BookingAgreementFromLedger
{
    public static Dictionary<string, double> SummarizeFallbackDeskLedgersForAgreement(List<FallbackDeskLedger> items)
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

    public static List<BookingAgreement> DeriveBookingAgreementsFromLedger(List<FallbackDeskLedger> items)
    {
        var outValue = new List<BookingAgreement>();
        foreach (var item in items)
        {
            var derived = BookingAgreement.NewBookingAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleBookingAgreement(BookingAgreement item)
    {
        return DeskManifestPolicy.DeskManifestBucket(item.NetLoad());
    }
}

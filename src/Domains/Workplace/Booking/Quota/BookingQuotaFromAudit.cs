namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Functions of booking quota from audit.</summary>
public static class BookingQuotaFromAudit
{
    public static Dictionary<string, double> SummarizeStrictBookingAuditsForQuota(List<StrictBookingAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<BookingQuota> DeriveBookingQuotasFromAudit(List<StrictBookingAudit> items)
    {
        var outValue = new List<BookingQuota>();
        foreach (var item in items)
        {
            var derived = BookingQuota.NewBookingQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceBookingQuota(BookingQuota item)
    {
        return BookingBatchPolicy.ClassifyBookingBatch(item.AdjustedVolume());
    }
}

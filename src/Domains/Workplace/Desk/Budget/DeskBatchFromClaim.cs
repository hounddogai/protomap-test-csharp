namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Functions of desk batch from claim.</summary>
public static class DeskBatchFromClaim
{
    public static Dictionary<string, double> SummarizeBookingClaimsForBatch(List<BookingClaim> items)
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

    public static List<DeskBatch> DeriveDeskBatchesFromClaim(List<BookingClaim> items)
    {
        var outValue = new List<DeskBatch>();
        foreach (var item in items)
        {
            var derived = DeskBatch.NewDeskBatch(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeDeskBatch(DeskBatch item)
    {
        return BookingProfilePolicy.BookingProfileBucket(item.EffectiveExposure());
    }
}

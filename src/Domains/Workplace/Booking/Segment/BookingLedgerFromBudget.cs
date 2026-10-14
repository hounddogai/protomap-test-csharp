namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Functions of booking ledger from budget.</summary>
public static class BookingLedgerFromBudget
{
    public static Dictionary<string, double> SummarizeAccessBadgeBudgetsForLedger(List<AccessBadgeBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<BookingLedger> DeriveBookingLedgersFromBudget(List<AccessBadgeBudget> items)
    {
        var outValue = new List<BookingLedger>();
        foreach (var item in items)
        {
            var derived = BookingLedger.NewBookingLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBookingLedger(BookingLedger item)
    {
        return AccessBadgeSnapshotPolicy.NormalizeAccessBadgeSnapshotCode(item.Code);
    }
}

namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Functions of rolling desk mapping from metric.</summary>
public static class RollingDeskMappingFromMetric
{
    public static Dictionary<string, double> SummarizePrimaryBookingMetricsForMapping(List<PrimaryBookingMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RollingDeskMapping> DeriveRollingDeskMappingsFromMetric(List<PrimaryBookingMetric> items)
    {
        var outValue = new List<RollingDeskMapping>();
        foreach (var item in items)
        {
            var derived = RollingDeskMapping.NewRollingDeskMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRollingDeskMapping(RollingDeskMapping item)
    {
        return BookingLedgerPolicy.NormalizeBookingLedgerCode(item.Code);
    }
}

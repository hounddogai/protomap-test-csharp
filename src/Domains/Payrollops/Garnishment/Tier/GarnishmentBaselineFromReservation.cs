namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Functions of garnishment baseline from reservation.</summary>
public static class GarnishmentBaselineFromReservation
{
    public static Dictionary<string, double> SummarizePayRunReservationsForBaseline(List<PayRunReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<GarnishmentBaseline> DeriveGarnishmentBaselinesFromReservation(List<PayRunReservation> items)
    {
        var outValue = new List<GarnishmentBaseline>();
        foreach (var item in items)
        {
            var derived = GarnishmentBaseline.NewGarnishmentBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankGarnishmentBaseline(GarnishmentBaseline item)
    {
        return PayRunReservationPolicy.PayRunReservationWeight(2, true);
    }
}

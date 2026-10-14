namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Functions of scheduled tax form metric from reservation.</summary>
public static class ScheduledTaxFormMetricFromReservation
{
    public static Dictionary<string, double> SummarizeCappedPayRunReservationsForMetric(List<CappedPayRunReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<ScheduledTaxFormMetric> DeriveScheduledTaxFormMetricsFromReservation(List<CappedPayRunReservation> items)
    {
        var outValue = new List<ScheduledTaxFormMetric>();
        foreach (var item in items)
        {
            var derived = ScheduledTaxFormMetric.NewScheduledTaxFormMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanScheduledTaxFormMetric(ScheduledTaxFormMetric item)
    {
        return LenientPayRunLedgerPolicy.LenientPayRunLedgerWeight(8, true);
    }
}

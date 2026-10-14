namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Functions of fallback payslip batch from reservation.</summary>
public static class FallbackPayslipBatchFromReservation
{
    public static Dictionary<string, double> SummarizeCappedRetroPayReservationsForBatch(List<CappedRetroPayReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<FallbackPayslipBatch> DeriveFallbackPayslipBatchesFromReservation(List<CappedRetroPayReservation> items)
    {
        var outValue = new List<FallbackPayslipBatch>();
        foreach (var item in items)
        {
            var derived = FallbackPayslipBatch.NewFallbackPayslipBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreFallbackPayslipBatch(FallbackPayslipBatch item)
    {
        return CappedRetroPayReservationPolicy.CappedRetroPayReservationWeight(2, true);
    }
}

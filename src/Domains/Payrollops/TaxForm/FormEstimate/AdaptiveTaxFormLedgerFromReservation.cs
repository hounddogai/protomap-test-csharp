namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Functions of adaptive tax form ledger from reservation.</summary>
public static class AdaptiveTaxFormLedgerFromReservation
{
    public static Dictionary<string, double> SummarizeGlobalDeductionReservationsForLedger(List<GlobalDeductionReservation> items)
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

    public static List<AdaptiveTaxFormLedger> DeriveAdaptiveTaxFormLedgersFromReservation(List<GlobalDeductionReservation> items)
    {
        var outValue = new List<AdaptiveTaxFormLedger>();
        foreach (var item in items)
        {
            var derived = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyAdaptiveTaxFormLedgerExtra(AdaptiveTaxFormLedger item)
    {
        return GlobalDeductionReservationPolicy.GlobalDeductionReservationBucket(item.EffectiveVolume());
    }
}

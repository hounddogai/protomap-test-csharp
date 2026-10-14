namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Functions of regional tax form estimate from reservation.</summary>
public static class RegionalTaxFormEstimateFromReservation
{
    public static Dictionary<string, double> SummarizeCappedRetroPayReservationsForEstimate(List<CappedRetroPayReservation> items)
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

    public static List<RegionalTaxFormEstimate> DeriveRegionalTaxFormEstimatesFromReservation(List<CappedRetroPayReservation> items)
    {
        var outValue = new List<RegionalTaxFormEstimate>();
        foreach (var item in items)
        {
            var derived = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateRegionalTaxFormEstimate(RegionalTaxFormEstimate item)
    {
        return CappedRetroPayReservationPolicy.CappedRetroPayReservationWeight(3, true);
    }
}

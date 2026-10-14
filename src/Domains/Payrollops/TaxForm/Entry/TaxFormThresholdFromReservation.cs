namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Functions of tax form threshold from reservation.</summary>
public static class TaxFormThresholdFromReservation
{
    public static Dictionary<string, double> SummarizeDeductionReservationsForThreshold(List<DeductionReservation> items)
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

    public static List<TaxFormThreshold> DeriveTaxFormThresholdsFromReservation(List<DeductionReservation> items)
    {
        var outValue = new List<TaxFormThreshold>();
        foreach (var item in items)
        {
            var derived = TaxFormThreshold.NewTaxFormThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeTaxFormThreshold(TaxFormThreshold item)
    {
        return DeductionReservationPolicy.NormalizeDeductionReservationCode(item.Code);
    }
}

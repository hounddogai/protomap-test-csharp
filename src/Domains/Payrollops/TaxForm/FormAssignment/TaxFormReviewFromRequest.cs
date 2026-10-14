namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Functions of tax form review from request.</summary>
public static class TaxFormReviewFromRequest
{
    public static Dictionary<string, double> SummarizeScheduledPayRunRequestsForReview(List<ScheduledPayRunRequest> items)
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

    public static List<TaxFormReview> DeriveTaxFormReviewsFromRequest(List<ScheduledPayRunRequest> items)
    {
        var outValue = new List<TaxFormReview>();
        foreach (var item in items)
        {
            var derived = TaxFormReview.NewTaxFormReview(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTaxFormReview(TaxFormReview item)
    {
        return LenientPayRunRequestPolicy.NormalizeLenientPayRunRequestCode(item.Code);
    }
}

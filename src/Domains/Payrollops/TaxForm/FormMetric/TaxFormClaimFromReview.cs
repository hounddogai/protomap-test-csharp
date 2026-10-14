namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of tax form claim from review.</summary>
public static class TaxFormClaimFromReview
{
    public static Dictionary<string, double> SummarizeSeasonalRetroPayReviewsForClaim(List<SeasonalRetroPayReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TaxFormClaim> DeriveTaxFormClaimsFromReview(List<SeasonalRetroPayReview> items)
    {
        var outValue = new List<TaxFormClaim>();
        foreach (var item in items)
        {
            var derived = TaxFormClaim.NewTaxFormClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeTaxFormClaim(TaxFormClaim item)
    {
        return RetroPayCheckpointPolicy.NormalizeRetroPayCheckpointCode(item.Code);
    }
}

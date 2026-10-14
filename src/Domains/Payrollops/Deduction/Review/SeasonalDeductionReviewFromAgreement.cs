namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Functions of seasonal deduction review from agreement.</summary>
public static class SeasonalDeductionReviewFromAgreement
{
    public static Dictionary<string, double> SummarizeRegionalGarnishmentAgreementsForReview(List<RegionalGarnishmentAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalDeductionReview> DeriveSeasonalDeductionReviewsFromAgreement(List<RegionalGarnishmentAgreement> items)
    {
        var outValue = new List<SeasonalDeductionReview>();
        foreach (var item in items)
        {
            var derived = SeasonalDeductionReview.NewSeasonalDeductionReview(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateSeasonalDeductionReview(SeasonalDeductionReview item)
    {
        return AdaptiveGarnishmentExceptionPolicy.AdaptiveGarnishmentExceptionWeight(3, true);
    }
}

namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Functions of adaptive balance baseline from review.</summary>
public static class AdaptiveBalanceBaselineFromReview
{
    public static Dictionary<string, double> SummarizeAccrualReviewsForBaseline(List<AccrualReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<AdaptiveBalanceBaseline> DeriveAdaptiveBalanceBaselinesFromReview(List<AccrualReview> items)
    {
        var outValue = new List<AdaptiveBalanceBaseline>();
        foreach (var item in items)
        {
            var derived = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAdaptiveBalanceBaseline(AdaptiveBalanceBaseline item)
    {
        return PrimaryAccrualManifestPolicy.ClassifyPrimaryAccrualManifest(item.MeasuredValue());
    }
}

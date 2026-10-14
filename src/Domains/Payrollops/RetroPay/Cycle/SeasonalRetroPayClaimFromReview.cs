namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Functions of seasonal retro pay claim from review.</summary>
public static class SeasonalRetroPayClaimFromReview
{
    public static Dictionary<string, double> SummarizeSeasonalDeductionReviewsForClaim(List<SeasonalDeductionReview> items)
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

    public static List<SeasonalRetroPayClaim> DeriveSeasonalRetroPayClaimsFromReview(List<SeasonalDeductionReview> items)
    {
        var outValue = new List<SeasonalRetroPayClaim>();
        foreach (var item in items)
        {
            var derived = SeasonalRetroPayClaim.NewSeasonalRetroPayClaim(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanSeasonalRetroPayClaim(SeasonalRetroPayClaim item)
    {
        return ManualDeductionNoticePolicy.ManualDeductionNoticeBucket(item.NetLoad());
    }
}

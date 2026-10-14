namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Functions of retro pay review from budget.</summary>
public static class RetroPayReviewFromBudget
{
    public static Dictionary<string, double> SummarizeDeductionBudgetsForReview(List<DeductionBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<RetroPayReview> DeriveRetroPayReviewsFromBudget(List<DeductionBudget> items)
    {
        var outValue = new List<RetroPayReview>();
        foreach (var item in items)
        {
            var derived = RetroPayReview.NewRetroPayReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectRetroPayReview(RetroPayReview item)
    {
        return DeductionBudgetPolicy.DeductionBudgetWeight(6, true);
    }
}

namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Functions of default payslip review from budget.</summary>
public static class DefaultPayslipReviewFromBudget
{
    public static Dictionary<string, double> SummarizeWeightedGarnishmentBudgetsForReview(List<WeightedGarnishmentBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<DefaultPayslipReview> DeriveDefaultPayslipReviewsFromBudget(List<WeightedGarnishmentBudget> items)
    {
        var outValue = new List<DefaultPayslipReview>();
        foreach (var item in items)
        {
            var derived = DefaultPayslipReview.NewDefaultPayslipReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankDefaultPayslipReview(DefaultPayslipReview item)
    {
        return StrictGarnishmentSnapshotPolicy.StrictGarnishmentSnapshotBucket(item.WeightedVolume());
    }
}

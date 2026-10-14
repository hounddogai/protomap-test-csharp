namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted garnishment review policy.</summary>
public static class WeightedGarnishmentReviewPolicy
{
    public static List<string> MergeWeightedGarnishmentReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 79958.0, in order.</summary>
    public static int WeightedGarnishmentReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79958.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

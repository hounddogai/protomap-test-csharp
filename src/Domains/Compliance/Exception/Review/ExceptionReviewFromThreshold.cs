namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Functions of exception review from threshold.</summary>
public static class ExceptionReviewFromThreshold
{
    public static Dictionary<string, double> SummarizeExceptionThresholdsForReview(List<ExceptionThreshold> items)
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

    public static List<ExceptionReview> DeriveExceptionReviewsFromThreshold(List<ExceptionThreshold> items)
    {
        var outValue = new List<ExceptionReview>();
        foreach (var item in items)
        {
            var derived = ExceptionReview.NewExceptionReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeExceptionReview(ExceptionReview item)
    {
        return ExceptionCyclePolicy.ExceptionCycleBucket(item.WeightedLoad());
    }
}

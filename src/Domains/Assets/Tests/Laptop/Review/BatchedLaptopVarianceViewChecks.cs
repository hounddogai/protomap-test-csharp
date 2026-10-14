namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BatchedLaptopVarianceViewChecks
{
    public static void BatchedLaptopVarianceViewShowsTheStatusInCapitals()
    {
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("code-421", "title-903");
        var view = BatchedLaptopVarianceView.ToBatchedLaptopVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedLaptopVarianceView.ToBatchedLaptopVarianceViews(new List<BatchedLaptopVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedLaptopVarianceViewShowsTheStatusInCapitals();
    }
}

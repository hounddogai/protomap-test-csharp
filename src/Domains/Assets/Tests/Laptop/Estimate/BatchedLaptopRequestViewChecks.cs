namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class BatchedLaptopRequestViewChecks
{
    public static void BatchedLaptopRequestViewShowsTheStatusInCapitals()
    {
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("code-336", "label-114");
        var view = BatchedLaptopRequestView.ToBatchedLaptopRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedLaptopRequestView.ToBatchedLaptopRequestViews(new List<BatchedLaptopRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedLaptopRequestViewShowsTheStatusInCapitals();
    }
}

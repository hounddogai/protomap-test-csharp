namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class BatchedPayRunVarianceViewChecks
{
    public static void BatchedPayRunVarianceViewShowsTheStatusInCapitals()
    {
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("code-313", "title-46");
        var view = BatchedPayRunVarianceView.ToBatchedPayRunVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPayRunVarianceView.ToBatchedPayRunVarianceViews(new List<BatchedPayRunVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPayRunVarianceViewShowsTheStatusInCapitals();
    }
}

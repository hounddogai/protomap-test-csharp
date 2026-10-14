namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalRetroPayBatchViewChecks
{
    public static void GlobalRetroPayBatchViewShowsTheStatusInCapitals()
    {
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("code-372", "name-187");
        var view = GlobalRetroPayBatchView.ToGlobalRetroPayBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalRetroPayBatchView.ToGlobalRetroPayBatchViews(new List<GlobalRetroPayBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalRetroPayBatchViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackPayRunBatchViewChecks
{
    public static void FallbackPayRunBatchViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("code-20", "name-79");
        var view = FallbackPayRunBatchView.ToFallbackPayRunBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayRunBatchView.ToFallbackPayRunBatchViews(new List<FallbackPayRunBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayRunBatchViewShowsTheStatusInCapitals();
    }
}

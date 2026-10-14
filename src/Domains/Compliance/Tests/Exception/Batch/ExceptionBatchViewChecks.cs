namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ExceptionBatchViewChecks
{
    public static void ExceptionBatchViewShowsTheStatusInCapitals()
    {
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
        var view = ExceptionBatchView.ToExceptionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionBatchView.ToExceptionBatchViews(new List<ExceptionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionBatchViewShowsTheStatusInCapitals();
    }
}

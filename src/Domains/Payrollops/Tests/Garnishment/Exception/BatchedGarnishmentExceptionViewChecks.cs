namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedGarnishmentExceptionViewChecks
{
    public static void BatchedGarnishmentExceptionViewShowsTheStatusInCapitals()
    {
        var item = BatchedGarnishmentException.NewBatchedGarnishmentException("code-274", "title-694");
        var view = BatchedGarnishmentExceptionView.ToBatchedGarnishmentExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedGarnishmentExceptionView.ToBatchedGarnishmentExceptionViews(new List<BatchedGarnishmentException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedGarnishmentExceptionViewShowsTheStatusInCapitals();
    }
}

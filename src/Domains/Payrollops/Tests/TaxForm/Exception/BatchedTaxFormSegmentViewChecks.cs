namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedTaxFormSegmentViewChecks
{
    public static void BatchedTaxFormSegmentViewShowsTheStatusInCapitals()
    {
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("code-574", "label-392");
        var view = BatchedTaxFormSegmentView.ToBatchedTaxFormSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTaxFormSegmentView.ToBatchedTaxFormSegmentViews(new List<BatchedTaxFormSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTaxFormSegmentViewShowsTheStatusInCapitals();
    }
}

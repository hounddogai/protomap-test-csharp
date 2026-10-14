namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualSpanBatchViewChecks
{
    public static void ManualSpanBatchViewShowsTheStatusInCapitals()
    {
        var item = ManualSpanBatch.NewManualSpanBatch("code-386", "title-448");
        var view = ManualSpanBatchView.ToManualSpanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualSpanBatchView.ToManualSpanBatchViews(new List<ManualSpanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualSpanBatchViewShowsTheStatusInCapitals();
    }
}

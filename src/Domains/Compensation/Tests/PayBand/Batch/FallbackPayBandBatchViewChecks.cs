namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayBandBatchViewChecks
{
    public static void FallbackPayBandBatchViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("code-453", "title-281");
        var view = FallbackPayBandBatchView.ToFallbackPayBandBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayBandBatchView.ToFallbackPayBandBatchViews(new List<FallbackPayBandBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayBandBatchViewShowsTheStatusInCapitals();
    }
}

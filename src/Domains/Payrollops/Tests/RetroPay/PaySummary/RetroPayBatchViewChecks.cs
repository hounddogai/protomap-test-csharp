namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class RetroPayBatchViewChecks
{
    public static void RetroPayBatchViewShowsTheStatusInCapitals()
    {
        var item = RetroPayBatch.NewRetroPayBatch("code-747", "name-525");
        var view = RetroPayBatchView.ToRetroPayBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayBatchView.ToRetroPayBatchViews(new List<RetroPayBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayBatchViewShowsTheStatusInCapitals();
    }
}

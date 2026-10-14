namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBatchViewChecks
{
    public static void PayRunBatchViewShowsTheStatusInCapitals()
    {
        var item = PayRunBatch.NewPayRunBatch("code-401", "label-180");
        var view = PayRunBatchView.ToPayRunBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunBatchView.ToPayRunBatchViews(new List<PayRunBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunBatchViewShowsTheStatusInCapitals();
    }
}

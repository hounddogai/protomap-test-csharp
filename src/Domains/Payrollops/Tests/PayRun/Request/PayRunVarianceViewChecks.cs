namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PayRunVarianceViewChecks
{
    public static void PayRunVarianceViewShowsTheStatusInCapitals()
    {
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
        var view = PayRunVarianceView.ToPayRunVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunVarianceView.ToPayRunVarianceViews(new List<PayRunVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunVarianceViewShowsTheStatusInCapitals();
    }
}

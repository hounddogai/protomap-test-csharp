namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RollingRetroPaySummaryViewChecks
{
    public static void RollingRetroPaySummaryViewShowsTheStatusInCapitals()
    {
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("code-866", "name-490");
        var view = RollingRetroPaySummaryView.ToRollingRetroPaySummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRetroPaySummaryView.ToRollingRetroPaySummaryViews(new List<RollingRetroPaySummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRetroPaySummaryViewShowsTheStatusInCapitals();
    }
}

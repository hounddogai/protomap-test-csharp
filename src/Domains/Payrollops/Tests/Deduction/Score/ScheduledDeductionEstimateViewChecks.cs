namespace Acme.Workforce.Payrollops.Tests.Deduction.Score;

using Acme.Workforce.Payrollops.Deduction.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledDeductionEstimateViewChecks
{
    public static void ScheduledDeductionEstimateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
        var view = ScheduledDeductionEstimateView.ToScheduledDeductionEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledDeductionEstimateView.ToScheduledDeductionEstimateViews(new List<ScheduledDeductionEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledDeductionEstimateViewShowsTheStatusInCapitals();
    }
}

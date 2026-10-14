namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReturnPlanSummaryViewChecks
{
    public static void ReturnPlanSummaryViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
        var view = ReturnPlanSummaryView.ToReturnPlanSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanSummaryView.ToReturnPlanSummaryViews(new List<ReturnPlanSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanSummaryViewShowsTheStatusInCapitals();
    }
}

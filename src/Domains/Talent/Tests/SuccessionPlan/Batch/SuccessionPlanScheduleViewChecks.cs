namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanScheduleViewChecks
{
    public static void SuccessionPlanScheduleViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("code-387", "label-967");
        var view = SuccessionPlanScheduleView.ToSuccessionPlanScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanScheduleView.ToSuccessionPlanScheduleViews(new List<SuccessionPlanSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanScheduleViewShowsTheStatusInCapitals();
    }
}

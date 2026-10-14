namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledPoolPlanViewChecks
{
    public static void ScheduledPoolPlanViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("code-541", "name-377");
        var view = ScheduledPoolPlanView.ToScheduledPoolPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPoolPlanView.ToScheduledPoolPlanViews(new List<ScheduledPoolPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPoolPlanViewShowsTheStatusInCapitals();
    }
}

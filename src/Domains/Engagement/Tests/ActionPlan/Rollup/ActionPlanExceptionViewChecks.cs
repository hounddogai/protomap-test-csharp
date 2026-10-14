namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionViewChecks
{
    public static void ActionPlanExceptionViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
        var view = ActionPlanExceptionView.ToActionPlanExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanExceptionView.ToActionPlanExceptionViews(new List<ActionPlanException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanExceptionViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class FallbackReturnPlanAssignmentViewChecks
{
    public static void FallbackReturnPlanAssignmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("code-885", "label-835");
        var view = FallbackReturnPlanAssignmentView.ToFallbackReturnPlanAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackReturnPlanAssignmentView.ToFallbackReturnPlanAssignmentViews(new List<FallbackReturnPlanAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackReturnPlanAssignmentViewShowsTheStatusInCapitals();
    }
}

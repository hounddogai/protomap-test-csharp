namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentPlanViewChecks
{
    public static void AssignmentPlanViewShowsTheStatusInCapitals()
    {
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
        var view = AssignmentPlanView.ToAssignmentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentPlanView.ToAssignmentPlanViews(new List<AssignmentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentPlanViewShowsTheStatusInCapitals();
    }
}

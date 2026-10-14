namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssignmentBudgetViewChecks
{
    public static void AssignmentBudgetViewShowsTheStatusInCapitals()
    {
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
        var view = AssignmentBudgetView.ToAssignmentBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentBudgetView.ToAssignmentBudgetViews(new List<AssignmentBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentBudgetViewShowsTheStatusInCapitals();
    }
}

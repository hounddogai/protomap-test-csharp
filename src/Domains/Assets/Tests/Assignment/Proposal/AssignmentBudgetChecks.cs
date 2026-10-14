namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssignmentBudgetChecks
{
    public static void AssignmentBudgetStartsInDraft()
    {
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
        Check.Equal(item.Status, AssignmentBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentBudgetMovesToClosed()
    {
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
        try
        {
            item.MoveTo(AssignmentBudgetStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentBudgetStatus.Closed, "status");
    }

    public static void AssignmentBudgetRejectsMovesFromClosed()
    {
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
        item.Status = AssignmentBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(AssignmentBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AssignmentBudgetStartsInDraft();
        AssignmentBudgetMovesToClosed();
        AssignmentBudgetRejectsMovesFromClosed();
    }
}

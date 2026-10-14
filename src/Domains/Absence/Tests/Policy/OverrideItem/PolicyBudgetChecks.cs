namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyBudgetChecks
{
    public static void PolicyBudgetStartsInPlanned()
    {
        var item = PolicyBudget.NewPolicyBudget("code-932", "title-574");
        Check.Equal(item.Status, PolicyBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyBudgetMovesToCompleted()
    {
        var item = PolicyBudget.NewPolicyBudget("code-932", "title-574");
        try
        {
            item.MoveTo(PolicyBudgetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyBudgetStatus.Completed, "status");
    }

    public static void PolicyBudgetRejectsMovesFromCancelled()
    {
        var item = PolicyBudget.NewPolicyBudget("code-932", "title-574");
        item.Status = PolicyBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyBudgetStartsInPlanned();
        PolicyBudgetMovesToCompleted();
        PolicyBudgetRejectsMovesFromCancelled();
    }
}

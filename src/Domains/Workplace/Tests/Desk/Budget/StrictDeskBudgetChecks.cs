namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictDeskBudgetChecks
{
    public static void StrictDeskBudgetStartsInDraft()
    {
        var item = StrictDeskBudget.NewStrictDeskBudget("code-735", "label-509");
        Check.Equal(item.Status, StrictDeskBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeskBudgetMovesToActive()
    {
        var item = StrictDeskBudget.NewStrictDeskBudget("code-735", "label-509");
        try
        {
            item.MoveTo(StrictDeskBudgetStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeskBudgetStatus.Active, "status");
    }

    public static void StrictDeskBudgetRejectsMovesFromClosed()
    {
        var item = StrictDeskBudget.NewStrictDeskBudget("code-735", "label-509");
        item.Status = StrictDeskBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictDeskBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictDeskBudgetStartsInDraft();
        StrictDeskBudgetMovesToActive();
        StrictDeskBudgetRejectsMovesFromClosed();
    }
}

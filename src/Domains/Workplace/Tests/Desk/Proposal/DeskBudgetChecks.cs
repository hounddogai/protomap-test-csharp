namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskBudgetChecks
{
    public static void DeskBudgetStartsInCreated()
    {
        var item = DeskBudget.NewDeskBudget("code-669", "label-55");
        Check.Equal(item.Status, DeskBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskBudgetMovesToPublished()
    {
        var item = DeskBudget.NewDeskBudget("code-669", "label-55");
        try
        {
            item.MoveTo(DeskBudgetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskBudgetStatus.Published, "status");
    }

    public static void DeskBudgetRejectsMovesFromRetired()
    {
        var item = DeskBudget.NewDeskBudget("code-669", "label-55");
        item.Status = DeskBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(DeskBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeskBudgetStartsInCreated();
        DeskBudgetMovesToPublished();
        DeskBudgetRejectsMovesFromRetired();
    }
}

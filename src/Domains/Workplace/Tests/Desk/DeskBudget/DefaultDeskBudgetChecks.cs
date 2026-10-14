namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class DefaultDeskBudgetChecks
{
    public static void DefaultDeskBudgetStartsInCreated()
    {
        var item = DefaultDeskBudget.NewDefaultDeskBudget("code-175", "title-303");
        Check.Equal(item.Status, DefaultDeskBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultDeskBudgetMovesToValidated()
    {
        var item = DefaultDeskBudget.NewDefaultDeskBudget("code-175", "title-303");
        try
        {
            item.MoveTo(DefaultDeskBudgetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultDeskBudgetStatus.Validated, "status");
    }

    public static void DefaultDeskBudgetRejectsMovesFromRetired()
    {
        var item = DefaultDeskBudget.NewDefaultDeskBudget("code-175", "title-303");
        item.Status = DefaultDeskBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultDeskBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultDeskBudgetStartsInCreated();
        DefaultDeskBudgetMovesToValidated();
        DefaultDeskBudgetRejectsMovesFromRetired();
    }
}

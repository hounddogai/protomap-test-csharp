namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CappedMeritCycleBudgetChecks
{
    public static void CappedMeritCycleBudgetStartsInCreated()
    {
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("code-260", "name-872");
        Check.Equal(item.Status, CappedMeritCycleBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedMeritCycleBudgetMovesToValidated()
    {
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("code-260", "name-872");
        try
        {
            item.MoveTo(CappedMeritCycleBudgetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedMeritCycleBudgetStatus.Validated, "status");
    }

    public static void CappedMeritCycleBudgetRejectsMovesFromRetired()
    {
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("code-260", "name-872");
        item.Status = CappedMeritCycleBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedMeritCycleBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedMeritCycleBudgetStartsInCreated();
        CappedMeritCycleBudgetMovesToValidated();
        CappedMeritCycleBudgetRejectsMovesFromRetired();
    }
}

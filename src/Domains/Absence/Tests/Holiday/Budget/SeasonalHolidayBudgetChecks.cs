namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SeasonalHolidayBudgetChecks
{
    public static void SeasonalHolidayBudgetStartsInRequested()
    {
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("code-511", "title-774");
        Check.Equal(item.Status, SeasonalHolidayBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHolidayBudgetMovesToRevoked()
    {
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("code-511", "title-774");
        try
        {
            item.MoveTo(SeasonalHolidayBudgetStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHolidayBudgetStatus.Revoked, "status");
    }

    public static void SeasonalHolidayBudgetRejectsMovesFromExpired()
    {
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("code-511", "title-774");
        item.Status = SeasonalHolidayBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalHolidayBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalHolidayBudgetStartsInRequested();
        SeasonalHolidayBudgetMovesToRevoked();
        SeasonalHolidayBudgetRejectsMovesFromExpired();
    }
}

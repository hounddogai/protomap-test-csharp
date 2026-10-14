namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayBudgetChecks
{
    public static void HolidayBudgetStartsInOpen()
    {
        var item = HolidayBudget.NewHolidayBudget("code-496", "label-458");
        Check.Equal(item.Status, HolidayBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayBudgetMovesToResolved()
    {
        var item = HolidayBudget.NewHolidayBudget("code-496", "label-458");
        try
        {
            item.MoveTo(HolidayBudgetStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayBudgetStatus.Resolved, "status");
    }

    public static void HolidayBudgetRejectsMovesFromArchived()
    {
        var item = HolidayBudget.NewHolidayBudget("code-496", "label-458");
        item.Status = HolidayBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayBudgetStartsInOpen();
        HolidayBudgetMovesToResolved();
        HolidayBudgetRejectsMovesFromArchived();
    }
}

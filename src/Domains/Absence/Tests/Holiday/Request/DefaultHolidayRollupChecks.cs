namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultHolidayRollupChecks
{
    public static void DefaultHolidayRollupStartsInPlanned()
    {
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("code-725", "label-644");
        Check.Equal(item.Status, DefaultHolidayRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHolidayRollupMovesToCompleted()
    {
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("code-725", "label-644");
        try
        {
            item.MoveTo(DefaultHolidayRollupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHolidayRollupStatus.Completed, "status");
    }

    public static void DefaultHolidayRollupRejectsMovesFromCancelled()
    {
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("code-725", "label-644");
        item.Status = DefaultHolidayRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultHolidayRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultHolidayRollupStartsInPlanned();
        DefaultHolidayRollupMovesToCompleted();
        DefaultHolidayRollupRejectsMovesFromCancelled();
    }
}

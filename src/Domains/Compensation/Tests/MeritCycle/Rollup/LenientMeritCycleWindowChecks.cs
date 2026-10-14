namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientMeritCycleWindowChecks
{
    public static void LenientMeritCycleWindowStartsInDraft()
    {
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("code-276", "name-554");
        Check.Equal(item.Status, LenientMeritCycleWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientMeritCycleWindowMovesToSuspended()
    {
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("code-276", "name-554");
        try
        {
            item.MoveTo(LenientMeritCycleWindowStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientMeritCycleWindowStatus.Suspended, "status");
    }

    public static void LenientMeritCycleWindowRejectsMovesFromClosed()
    {
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("code-276", "name-554");
        item.Status = LenientMeritCycleWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientMeritCycleWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientMeritCycleWindowStartsInDraft();
        LenientMeritCycleWindowMovesToSuspended();
        LenientMeritCycleWindowRejectsMovesFromClosed();
    }
}

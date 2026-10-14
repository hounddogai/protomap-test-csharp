namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleExceptionChecks
{
    public static void MeritCycleExceptionStartsInDraft()
    {
        var item = MeritCycleException.NewMeritCycleException("code-984", "title-950");
        Check.Equal(item.Status, MeritCycleExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleExceptionMovesToSuspended()
    {
        var item = MeritCycleException.NewMeritCycleException("code-984", "title-950");
        try
        {
            item.MoveTo(MeritCycleExceptionStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleExceptionStatus.Suspended, "status");
    }

    public static void MeritCycleExceptionRejectsMovesFromClosed()
    {
        var item = MeritCycleException.NewMeritCycleException("code-984", "title-950");
        item.Status = MeritCycleExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleExceptionStartsInDraft();
        MeritCycleExceptionMovesToSuspended();
        MeritCycleExceptionRejectsMovesFromClosed();
    }
}

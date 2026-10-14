namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class FallbackPayRunCycleChecks
{
    public static void FallbackPayRunCycleStartsInPlanned()
    {
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("code-163", "title-577");
        Check.Equal(item.Status, FallbackPayRunCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayRunCycleMovesToInProgress()
    {
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("code-163", "title-577");
        try
        {
            item.MoveTo(FallbackPayRunCycleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayRunCycleStatus.InProgress, "status");
    }

    public static void FallbackPayRunCycleRejectsMovesFromCancelled()
    {
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("code-163", "title-577");
        item.Status = FallbackPayRunCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPayRunCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPayRunCycleStartsInPlanned();
        FallbackPayRunCycleMovesToInProgress();
        FallbackPayRunCycleRejectsMovesFromCancelled();
    }
}

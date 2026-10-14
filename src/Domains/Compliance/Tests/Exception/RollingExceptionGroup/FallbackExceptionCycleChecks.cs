namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class FallbackExceptionCycleChecks
{
    public static void FallbackExceptionCycleStartsInPlanned()
    {
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("code-357", "label-97");
        Check.Equal(item.Status, FallbackExceptionCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackExceptionCycleMovesToInProgress()
    {
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("code-357", "label-97");
        try
        {
            item.MoveTo(FallbackExceptionCycleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackExceptionCycleStatus.InProgress, "status");
    }

    public static void FallbackExceptionCycleRejectsMovesFromCancelled()
    {
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("code-357", "label-97");
        item.Status = FallbackExceptionCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackExceptionCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackExceptionCycleStartsInPlanned();
        FallbackExceptionCycleMovesToInProgress();
        FallbackExceptionCycleRejectsMovesFromCancelled();
    }
}

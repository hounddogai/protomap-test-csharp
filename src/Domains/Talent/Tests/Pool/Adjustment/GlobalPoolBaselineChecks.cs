namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalPoolBaselineChecks
{
    public static void GlobalPoolBaselineStartsInPlanned()
    {
        var item = GlobalPoolBaseline.NewGlobalPoolBaseline("code-309", "name-983");
        Check.Equal(item.Status, GlobalPoolBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPoolBaselineMovesToScheduled()
    {
        var item = GlobalPoolBaseline.NewGlobalPoolBaseline("code-309", "name-983");
        try
        {
            item.MoveTo(GlobalPoolBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPoolBaselineStatus.Scheduled, "status");
    }

    public static void GlobalPoolBaselineRejectsMovesFromCancelled()
    {
        var item = GlobalPoolBaseline.NewGlobalPoolBaseline("code-309", "name-983");
        item.Status = GlobalPoolBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalPoolBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalPoolBaselineStartsInPlanned();
        GlobalPoolBaselineMovesToScheduled();
        GlobalPoolBaselineRejectsMovesFromCancelled();
    }
}

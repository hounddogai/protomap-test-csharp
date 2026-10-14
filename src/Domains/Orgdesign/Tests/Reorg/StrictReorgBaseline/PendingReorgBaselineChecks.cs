namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class PendingReorgBaselineChecks
{
    public static void PendingReorgBaselineStartsInIdle()
    {
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
        Check.Equal(item.Status, PendingReorgBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReorgBaselineMovesToWarming()
    {
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
        try
        {
            item.MoveTo(PendingReorgBaselineStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReorgBaselineStatus.Warming, "status");
    }

    public static void PendingReorgBaselineRejectsMovesFromStopped()
    {
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
        item.Status = PendingReorgBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingReorgBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingReorgBaselineStartsInIdle();
        PendingReorgBaselineMovesToWarming();
        PendingReorgBaselineRejectsMovesFromStopped();
    }
}

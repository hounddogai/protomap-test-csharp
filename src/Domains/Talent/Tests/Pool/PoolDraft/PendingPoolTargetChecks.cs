namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class PendingPoolTargetChecks
{
    public static void PendingPoolTargetStartsInPlanned()
    {
        var item = PendingPoolTarget.NewPendingPoolTarget("code-882", "label-191");
        Check.Equal(item.Status, PendingPoolTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPoolTargetMovesToInProgress()
    {
        var item = PendingPoolTarget.NewPendingPoolTarget("code-882", "label-191");
        try
        {
            item.MoveTo(PendingPoolTargetStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPoolTargetStatus.InProgress, "status");
    }

    public static void PendingPoolTargetRejectsMovesFromCancelled()
    {
        var item = PendingPoolTarget.NewPendingPoolTarget("code-882", "label-191");
        item.Status = PendingPoolTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingPoolTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingPoolTargetStartsInPlanned();
        PendingPoolTargetMovesToInProgress();
        PendingPoolTargetRejectsMovesFromCancelled();
    }
}

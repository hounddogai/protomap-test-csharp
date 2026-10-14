namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupChecks
{
    public static void ReorgRollupStartsInPlanned()
    {
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
        Check.Equal(item.Status, ReorgRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgRollupMovesToInProgress()
    {
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
        try
        {
            item.MoveTo(ReorgRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgRollupStatus.InProgress, "status");
    }

    public static void ReorgRollupRejectsMovesFromCancelled()
    {
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
        item.Status = ReorgRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReorgRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReorgRollupStartsInPlanned();
        ReorgRollupMovesToInProgress();
        ReorgRollupRejectsMovesFromCancelled();
    }
}

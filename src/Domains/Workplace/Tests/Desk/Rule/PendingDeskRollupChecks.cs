namespace Acme.Workforce.Workplace.Tests.Desk.Rule;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingDeskRollupChecks
{
    public static void PendingDeskRollupStartsInNew()
    {
        var item = PendingDeskRollup.NewPendingDeskRollup("code-767", "name-618");
        Check.Equal(item.Status, PendingDeskRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingDeskRollupMovesToQueued()
    {
        var item = PendingDeskRollup.NewPendingDeskRollup("code-767", "name-618");
        try
        {
            item.MoveTo(PendingDeskRollupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingDeskRollupStatus.Queued, "status");
    }

    public static void PendingDeskRollupRejectsMovesFromFailed()
    {
        var item = PendingDeskRollup.NewPendingDeskRollup("code-767", "name-618");
        item.Status = PendingDeskRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingDeskRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingDeskRollupStartsInNew();
        PendingDeskRollupMovesToQueued();
        PendingDeskRollupRejectsMovesFromFailed();
    }
}

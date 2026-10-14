namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class GlobalEquityGrantDraftChecks
{
    public static void GlobalEquityGrantDraftStartsInIdle()
    {
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("code-531", "title-962");
        Check.Equal(item.Status, GlobalEquityGrantDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalEquityGrantDraftMovesToReady()
    {
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("code-531", "title-962");
        try
        {
            item.MoveTo(GlobalEquityGrantDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalEquityGrantDraftStatus.Ready, "status");
    }

    public static void GlobalEquityGrantDraftRejectsMovesFromStopped()
    {
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("code-531", "title-962");
        item.Status = GlobalEquityGrantDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalEquityGrantDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalEquityGrantDraftStartsInIdle();
        GlobalEquityGrantDraftMovesToReady();
        GlobalEquityGrantDraftRejectsMovesFromStopped();
    }
}

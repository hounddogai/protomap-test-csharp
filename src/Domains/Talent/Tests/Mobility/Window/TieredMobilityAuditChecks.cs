namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredMobilityAuditChecks
{
    public static void TieredMobilityAuditStartsInDraft()
    {
        var item = TieredMobilityAudit.NewTieredMobilityAudit("code-772", "name-766");
        Check.Equal(item.Status, TieredMobilityAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredMobilityAuditMovesToActive()
    {
        var item = TieredMobilityAudit.NewTieredMobilityAudit("code-772", "name-766");
        try
        {
            item.MoveTo(TieredMobilityAuditStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredMobilityAuditStatus.Active, "status");
    }

    public static void TieredMobilityAuditRejectsMovesFromClosed()
    {
        var item = TieredMobilityAudit.NewTieredMobilityAudit("code-772", "name-766");
        item.Status = TieredMobilityAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredMobilityAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredMobilityAuditStartsInDraft();
        TieredMobilityAuditMovesToActive();
        TieredMobilityAuditRejectsMovesFromClosed();
    }
}

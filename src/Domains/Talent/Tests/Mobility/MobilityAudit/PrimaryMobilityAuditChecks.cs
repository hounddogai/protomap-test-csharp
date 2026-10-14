namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class PrimaryMobilityAuditChecks
{
    public static void PrimaryMobilityAuditStartsInDraft()
    {
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("code-678", "title-217");
        Check.Equal(item.Status, PrimaryMobilityAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMobilityAuditMovesToPending()
    {
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("code-678", "title-217");
        try
        {
            item.MoveTo(PrimaryMobilityAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMobilityAuditStatus.Pending, "status");
    }

    public static void PrimaryMobilityAuditRejectsMovesFromClosed()
    {
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("code-678", "title-217");
        item.Status = PrimaryMobilityAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryMobilityAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryMobilityAuditStartsInDraft();
        PrimaryMobilityAuditMovesToPending();
        PrimaryMobilityAuditRejectsMovesFromClosed();
    }
}

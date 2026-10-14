namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityAuditChecks
{
    public static void MobilityAuditStartsInRequested()
    {
        var item = MobilityAudit.NewMobilityAudit("code-253", "title-965");
        Check.Equal(item.Status, MobilityAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityAuditMovesToExpired()
    {
        var item = MobilityAudit.NewMobilityAudit("code-253", "title-965");
        try
        {
            item.MoveTo(MobilityAuditStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityAuditStatus.Expired, "status");
    }

    public static void MobilityAuditRejectsMovesFromExpired()
    {
        var item = MobilityAudit.NewMobilityAudit("code-253", "title-965");
        item.Status = MobilityAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(MobilityAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MobilityAuditStartsInRequested();
        MobilityAuditMovesToExpired();
        MobilityAuditRejectsMovesFromExpired();
    }
}

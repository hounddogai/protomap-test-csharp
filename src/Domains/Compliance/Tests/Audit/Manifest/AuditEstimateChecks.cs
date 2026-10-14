namespace Acme.Workforce.Compliance.Tests.Audit.Manifest;

using Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AuditEstimateChecks
{
    public static void AuditEstimateStartsInNew()
    {
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
        Check.Equal(item.Status, AuditEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditEstimateMovesToDone()
    {
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
        try
        {
            item.MoveTo(AuditEstimateStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditEstimateStatus.Done, "status");
    }

    public static void AuditEstimateRejectsMovesFromFailed()
    {
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
        item.Status = AuditEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(AuditEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        AuditEstimateStartsInNew();
        AuditEstimateMovesToDone();
        AuditEstimateRejectsMovesFromFailed();
    }
}

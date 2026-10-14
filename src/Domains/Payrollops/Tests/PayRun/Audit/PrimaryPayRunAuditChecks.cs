namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryPayRunAuditChecks
{
    public static void PrimaryPayRunAuditStartsInOpen()
    {
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("code-10", "title-564");
        Check.Equal(item.Status, PrimaryPayRunAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPayRunAuditMovesToInReview()
    {
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("code-10", "title-564");
        try
        {
            item.MoveTo(PrimaryPayRunAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPayRunAuditStatus.InReview, "status");
    }

    public static void PrimaryPayRunAuditRejectsMovesFromArchived()
    {
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("code-10", "title-564");
        item.Status = PrimaryPayRunAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryPayRunAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryPayRunAuditStartsInOpen();
        PrimaryPayRunAuditMovesToInReview();
        PrimaryPayRunAuditRejectsMovesFromArchived();
    }
}

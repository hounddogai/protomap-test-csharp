namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RegionalExceptionAuditChecks
{
    public static void RegionalExceptionAuditStartsInOpen()
    {
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("code-972", "name-764");
        Check.Equal(item.Status, RegionalExceptionAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalExceptionAuditMovesToInReview()
    {
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("code-972", "name-764");
        try
        {
            item.MoveTo(RegionalExceptionAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalExceptionAuditStatus.InReview, "status");
    }

    public static void RegionalExceptionAuditRejectsMovesFromArchived()
    {
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("code-972", "name-764");
        item.Status = RegionalExceptionAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalExceptionAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalExceptionAuditStartsInOpen();
        RegionalExceptionAuditMovesToInReview();
        RegionalExceptionAuditRejectsMovesFromArchived();
    }
}

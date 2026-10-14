namespace Acme.Workforce.Payrollops.Tests.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Checks of legacy garnishment agreement code, run by the project's runner.</summary>
public static class GarnishmentAuditChecks
{
    public static void GarnishmentAuditStartsInOpen()
    {
        var item = GarnishmentAudit.NewGarnishmentAudit("code-587", "name-678");
        Check.Equal(item.Status, GarnishmentAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentAuditMovesToInReview()
    {
        var item = GarnishmentAudit.NewGarnishmentAudit("code-587", "name-678");
        try
        {
            item.MoveTo(GarnishmentAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentAuditStatus.InReview, "status");
    }

    public static void GarnishmentAuditRejectsMovesFromArchived()
    {
        var item = GarnishmentAudit.NewGarnishmentAudit("code-587", "name-678");
        item.Status = GarnishmentAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(GarnishmentAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        GarnishmentAuditStartsInOpen();
        GarnishmentAuditMovesToInReview();
        GarnishmentAuditRejectsMovesFromArchived();
    }
}

namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormAuditChecks
{
    public static void StrictTaxFormAuditStartsInOpen()
    {
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("code-177", "title-117");
        Check.Equal(item.Status, StrictTaxFormAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTaxFormAuditMovesToReopened()
    {
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("code-177", "title-117");
        try
        {
            item.MoveTo(StrictTaxFormAuditStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTaxFormAuditStatus.Reopened, "status");
    }

    public static void StrictTaxFormAuditRejectsMovesFromArchived()
    {
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("code-177", "title-117");
        item.Status = StrictTaxFormAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictTaxFormAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictTaxFormAuditStartsInOpen();
        StrictTaxFormAuditMovesToReopened();
        StrictTaxFormAuditRejectsMovesFromArchived();
    }
}

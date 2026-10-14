namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormAuditChecks
{
    public static void TaxFormAuditStartsInCreated()
    {
        var item = TaxFormAudit.NewTaxFormAudit("code-560", "title-721");
        Check.Equal(item.Status, TaxFormAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormAuditMovesToValidated()
    {
        var item = TaxFormAudit.NewTaxFormAudit("code-560", "title-721");
        try
        {
            item.MoveTo(TaxFormAuditStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormAuditStatus.Validated, "status");
    }

    public static void TaxFormAuditRejectsMovesFromRetired()
    {
        var item = TaxFormAudit.NewTaxFormAudit("code-560", "title-721");
        item.Status = TaxFormAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(TaxFormAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        TaxFormAuditStartsInCreated();
        TaxFormAuditMovesToValidated();
        TaxFormAuditRejectsMovesFromRetired();
    }
}

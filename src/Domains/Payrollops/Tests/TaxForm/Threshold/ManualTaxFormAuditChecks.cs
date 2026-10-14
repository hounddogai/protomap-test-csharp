namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualTaxFormAuditChecks
{
    public static void ManualTaxFormAuditStartsInIdle()
    {
        var item = ManualTaxFormAudit.NewManualTaxFormAudit("code-159", "name-971");
        Check.Equal(item.Status, ManualTaxFormAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualTaxFormAuditMovesToWarming()
    {
        var item = ManualTaxFormAudit.NewManualTaxFormAudit("code-159", "name-971");
        try
        {
            item.MoveTo(ManualTaxFormAuditStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualTaxFormAuditStatus.Warming, "status");
    }

    public static void ManualTaxFormAuditRejectsMovesFromStopped()
    {
        var item = ManualTaxFormAudit.NewManualTaxFormAudit("code-159", "name-971");
        item.Status = ManualTaxFormAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualTaxFormAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualTaxFormAuditStartsInIdle();
        ManualTaxFormAuditMovesToWarming();
        ManualTaxFormAuditRejectsMovesFromStopped();
    }
}

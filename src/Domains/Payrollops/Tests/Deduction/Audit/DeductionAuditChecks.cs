namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionAuditChecks
{
    public static void DeductionAuditStartsInCreated()
    {
        var item = DeductionAudit.NewDeductionAudit("code-688", "name-676");
        Check.Equal(item.Status, DeductionAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionAuditMovesToPublished()
    {
        var item = DeductionAudit.NewDeductionAudit("code-688", "name-676");
        try
        {
            item.MoveTo(DeductionAuditStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionAuditStatus.Published, "status");
    }

    public static void DeductionAuditRejectsMovesFromRetired()
    {
        var item = DeductionAudit.NewDeductionAudit("code-688", "name-676");
        item.Status = DeductionAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(DeductionAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeductionAuditStartsInCreated();
        DeductionAuditMovesToPublished();
        DeductionAuditRejectsMovesFromRetired();
    }
}

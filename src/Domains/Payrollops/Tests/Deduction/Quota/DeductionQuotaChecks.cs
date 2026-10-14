namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DeductionQuotaChecks
{
    public static void DeductionQuotaStartsInCreated()
    {
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
        Check.Equal(item.Status, DeductionQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionQuotaMovesToValidated()
    {
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
        try
        {
            item.MoveTo(DeductionQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionQuotaStatus.Validated, "status");
    }

    public static void DeductionQuotaRejectsMovesFromRetired()
    {
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
        item.Status = DeductionQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(DeductionQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeductionQuotaStartsInCreated();
        DeductionQuotaMovesToValidated();
        DeductionQuotaRejectsMovesFromRetired();
    }
}

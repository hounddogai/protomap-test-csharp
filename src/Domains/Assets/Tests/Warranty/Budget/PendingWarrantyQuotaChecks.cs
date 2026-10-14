namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingWarrantyQuotaChecks
{
    public static void PendingWarrantyQuotaStartsInNew()
    {
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("code-388", "label-383");
        Check.Equal(item.Status, PendingWarrantyQuotaStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingWarrantyQuotaMovesToDone()
    {
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("code-388", "label-383");
        try
        {
            item.MoveTo(PendingWarrantyQuotaStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingWarrantyQuotaStatus.Done, "status");
    }

    public static void PendingWarrantyQuotaRejectsMovesFromFailed()
    {
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("code-388", "label-383");
        item.Status = PendingWarrantyQuotaStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingWarrantyQuotaStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingWarrantyQuotaStartsInNew();
        PendingWarrantyQuotaMovesToDone();
        PendingWarrantyQuotaRejectsMovesFromFailed();
    }
}

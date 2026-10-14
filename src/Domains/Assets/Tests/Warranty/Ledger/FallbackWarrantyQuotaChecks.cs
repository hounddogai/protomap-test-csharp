namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackWarrantyQuotaChecks
{
    public static void FallbackWarrantyQuotaStartsInCreated()
    {
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("code-94", "title-97");
        Check.Equal(item.Status, FallbackWarrantyQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackWarrantyQuotaMovesToPublished()
    {
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("code-94", "title-97");
        try
        {
            item.MoveTo(FallbackWarrantyQuotaStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackWarrantyQuotaStatus.Published, "status");
    }

    public static void FallbackWarrantyQuotaRejectsMovesFromRetired()
    {
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("code-94", "title-97");
        item.Status = FallbackWarrantyQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackWarrantyQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackWarrantyQuotaStartsInCreated();
        FallbackWarrantyQuotaMovesToPublished();
        FallbackWarrantyQuotaRejectsMovesFromRetired();
    }
}

namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgClaimStoreChecks
{
    public static void MemoryReorgClaimStoreFindsWhatItSaved()
    {
        var store = ReorgClaimStore.NewMemoryReorgClaimStore();
        var item = ReorgClaim.NewReorgClaim("code-906", "title-918");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryReorgClaimStoreFindsWhatItSaved();
    }
}

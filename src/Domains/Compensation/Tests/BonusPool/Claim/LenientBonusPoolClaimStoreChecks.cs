namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientBonusPoolClaimStoreChecks
{
    public static void MemoryLenientBonusPoolClaimStoreFindsWhatItSaved()
    {
        var store = LenientBonusPoolClaimStore.NewMemoryLenientBonusPoolClaimStore();
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("code-6", "name-514");
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
        MemoryLenientBonusPoolClaimStoreFindsWhatItSaved();
    }
}

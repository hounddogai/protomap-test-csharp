namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountClaimStoreChecks
{
    public static void MemoryHeadcountClaimStoreFindsWhatItSaved()
    {
        var store = HeadcountClaimStore.NewMemoryHeadcountClaimStore();
        var item = HeadcountClaim.NewHeadcountClaim("code-248", "title-690");
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
        MemoryHeadcountClaimStoreFindsWhatItSaved();
    }
}

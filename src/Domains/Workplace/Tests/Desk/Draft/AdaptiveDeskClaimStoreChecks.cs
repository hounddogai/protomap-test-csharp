namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AdaptiveDeskClaimStoreChecks
{
    public static void MemoryAdaptiveDeskClaimStoreFindsWhatItSaved()
    {
        var store = AdaptiveDeskClaimStore.NewMemoryAdaptiveDeskClaimStore();
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("code-378", "label-486");
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
        MemoryAdaptiveDeskClaimStoreFindsWhatItSaved();
    }
}

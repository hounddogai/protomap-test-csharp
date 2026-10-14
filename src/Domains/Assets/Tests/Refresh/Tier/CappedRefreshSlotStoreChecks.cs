namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRefreshSlotStoreChecks
{
    public static void MemoryCappedRefreshSlotStoreFindsWhatItSaved()
    {
        var store = CappedRefreshSlotStore.NewMemoryCappedRefreshSlotStore();
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
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
        MemoryCappedRefreshSlotStoreFindsWhatItSaved();
    }
}

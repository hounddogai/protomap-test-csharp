namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RefreshAllocationStoreChecks
{
    public static void MemoryRefreshAllocationStoreFindsWhatItSaved()
    {
        var store = RefreshAllocationStore.NewMemoryRefreshAllocationStore();
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
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
        MemoryRefreshAllocationStoreFindsWhatItSaved();
    }
}

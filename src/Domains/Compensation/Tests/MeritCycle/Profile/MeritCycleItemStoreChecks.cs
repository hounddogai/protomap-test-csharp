namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleItemStoreChecks
{
    public static void MemoryMeritCycleItemStoreFindsWhatItSaved()
    {
        var store = MeritCycleItemStore.NewMemoryMeritCycleItemStore();
        var item = MeritCycleItem.NewMeritCycleItem("code-128", "label-991");
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
        MemoryMeritCycleItemStoreFindsWhatItSaved();
    }
}

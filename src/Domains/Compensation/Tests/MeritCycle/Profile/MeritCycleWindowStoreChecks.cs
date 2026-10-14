namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleWindowStoreChecks
{
    public static void MemoryMeritCycleWindowStoreFindsWhatItSaved()
    {
        var store = MeritCycleWindowStore.NewMemoryMeritCycleWindowStore();
        var item = MeritCycleWindow.NewMeritCycleWindow("code-538", "name-236");
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
        MemoryMeritCycleWindowStoreFindsWhatItSaved();
    }
}

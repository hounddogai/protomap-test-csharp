namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientMeritCycleWindowStoreChecks
{
    public static void MemoryLenientMeritCycleWindowStoreFindsWhatItSaved()
    {
        var store = LenientMeritCycleWindowStore.NewMemoryLenientMeritCycleWindowStore();
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("code-276", "name-554");
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
        MemoryLenientMeritCycleWindowStoreFindsWhatItSaved();
    }
}

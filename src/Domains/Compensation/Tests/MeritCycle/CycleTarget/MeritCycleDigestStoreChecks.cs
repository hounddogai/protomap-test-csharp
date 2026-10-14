namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleDigestStoreChecks
{
    public static void MemoryMeritCycleDigestStoreFindsWhatItSaved()
    {
        var store = MeritCycleDigestStore.NewMemoryMeritCycleDigestStore();
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
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
        MemoryMeritCycleDigestStoreFindsWhatItSaved();
    }
}

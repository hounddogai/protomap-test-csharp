namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaStoreChecks
{
    public static void MemoryMeritCycleQuotaStoreFindsWhatItSaved()
    {
        var store = MeritCycleQuotaStore.NewMemoryMeritCycleQuotaStore();
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
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
        MemoryMeritCycleQuotaStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeStoreChecks
{
    public static void MemoryMeritCycleNoticeStoreFindsWhatItSaved()
    {
        var store = MeritCycleNoticeStore.NewMemoryMeritCycleNoticeStore();
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
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
        MemoryMeritCycleNoticeStoreFindsWhatItSaved();
    }
}

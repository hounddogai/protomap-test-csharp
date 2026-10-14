namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityNoticeStoreChecks
{
    public static void MemoryRollingMobilityNoticeStoreFindsWhatItSaved()
    {
        var store = RollingMobilityNoticeStore.NewMemoryRollingMobilityNoticeStore();
        var item = RollingMobilityNotice.NewRollingMobilityNotice("code-192", "name-894");
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
        MemoryRollingMobilityNoticeStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingReorgNoticeStoreChecks
{
    public static void MemoryRollingReorgNoticeStoreFindsWhatItSaved()
    {
        var store = RollingReorgNoticeStore.NewMemoryRollingReorgNoticeStore();
        var item = RollingReorgNotice.NewRollingReorgNotice("code-717", "label-6");
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
        MemoryRollingReorgNoticeStoreFindsWhatItSaved();
    }
}

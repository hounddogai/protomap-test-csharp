namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredSpanScheduleStoreChecks
{
    public static void MemoryTieredSpanScheduleStoreFindsWhatItSaved()
    {
        var store = TieredSpanScheduleStore.NewMemoryTieredSpanScheduleStore();
        var item = TieredSpanSchedule.NewTieredSpanSchedule("code-728", "name-94");
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
        MemoryTieredSpanScheduleStoreFindsWhatItSaved();
    }
}

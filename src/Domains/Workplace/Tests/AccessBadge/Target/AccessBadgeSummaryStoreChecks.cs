namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeSummaryStoreChecks
{
    public static void MemoryAccessBadgeSummaryStoreFindsWhatItSaved()
    {
        var store = AccessBadgeSummaryStore.NewMemoryAccessBadgeSummaryStore();
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
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
        MemoryAccessBadgeSummaryStoreFindsWhatItSaved();
    }
}

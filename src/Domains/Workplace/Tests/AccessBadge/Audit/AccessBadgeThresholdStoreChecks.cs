namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeThresholdStoreChecks
{
    public static void MemoryAccessBadgeThresholdStoreFindsWhatItSaved()
    {
        var store = AccessBadgeThresholdStore.NewMemoryAccessBadgeThresholdStore();
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("code-498", "name-5");
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
        MemoryAccessBadgeThresholdStoreFindsWhatItSaved();
    }
}

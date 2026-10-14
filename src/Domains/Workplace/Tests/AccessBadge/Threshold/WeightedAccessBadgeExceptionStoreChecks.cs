namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedAccessBadgeExceptionStoreChecks
{
    public static void MemoryWeightedAccessBadgeExceptionStoreFindsWhatItSaved()
    {
        var store = WeightedAccessBadgeExceptionStore.NewMemoryWeightedAccessBadgeExceptionStore();
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
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
        MemoryWeightedAccessBadgeExceptionStoreFindsWhatItSaved();
    }
}

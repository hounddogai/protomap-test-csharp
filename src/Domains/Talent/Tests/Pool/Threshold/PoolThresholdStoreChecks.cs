namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolThresholdStoreChecks
{
    public static void MemoryPoolThresholdStoreFindsWhatItSaved()
    {
        var store = PoolThresholdStore.NewMemoryPoolThresholdStore();
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
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
        MemoryPoolThresholdStoreFindsWhatItSaved();
    }
}

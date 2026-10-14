namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolPreferenceStoreChecks
{
    public static void MemoryPoolPreferenceStoreFindsWhatItSaved()
    {
        var store = PoolPreferenceStore.NewMemoryPoolPreferenceStore();
        var item = PoolPreference.NewPoolPreference("code-581", "label-944");
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
        MemoryPoolPreferenceStoreFindsWhatItSaved();
    }
}

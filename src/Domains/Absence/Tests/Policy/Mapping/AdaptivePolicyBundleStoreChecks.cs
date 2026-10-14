namespace Acme.Workforce.Absence.Tests.Policy.Mapping;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptivePolicyBundleStoreChecks
{
    public static void MemoryAdaptivePolicyBundleStoreFindsWhatItSaved()
    {
        var store = AdaptivePolicyBundleStore.NewMemoryAdaptivePolicyBundleStore();
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
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
        MemoryAdaptivePolicyBundleStoreFindsWhatItSaved();
    }
}

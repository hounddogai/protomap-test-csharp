namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryOffboardingBatchStoreChecks
{
    public static void MemoryPrimaryOffboardingBatchStoreFindsWhatItSaved()
    {
        var store = PrimaryOffboardingBatchStore.NewMemoryPrimaryOffboardingBatchStore();
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("code-824", "name-368");
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
        MemoryPrimaryOffboardingBatchStoreFindsWhatItSaved();
    }
}

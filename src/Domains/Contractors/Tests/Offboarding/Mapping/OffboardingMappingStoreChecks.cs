namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingMappingStoreChecks
{
    public static void MemoryOffboardingMappingStoreFindsWhatItSaved()
    {
        var store = OffboardingMappingStore.NewMemoryOffboardingMappingStore();
        var item = OffboardingMapping.NewOffboardingMapping("code-647", "title-205");
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
        MemoryOffboardingMappingStoreFindsWhatItSaved();
    }
}

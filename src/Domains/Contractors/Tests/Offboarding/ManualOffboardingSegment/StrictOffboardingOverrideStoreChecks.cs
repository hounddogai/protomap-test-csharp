namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class StrictOffboardingOverrideStoreChecks
{
    public static void MemoryStrictOffboardingOverrideStoreFindsWhatItSaved()
    {
        var store = StrictOffboardingOverrideStore.NewMemoryStrictOffboardingOverrideStore();
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("code-553", "title-73");
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
        MemoryStrictOffboardingOverrideStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingThresholdStoreChecks
{
    public static void MemoryOffboardingThresholdStoreFindsWhatItSaved()
    {
        var store = OffboardingThresholdStore.NewMemoryOffboardingThresholdStore();
        var item = OffboardingThreshold.NewOffboardingThreshold("code-193", "title-959");
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
        MemoryOffboardingThresholdStoreFindsWhatItSaved();
    }
}

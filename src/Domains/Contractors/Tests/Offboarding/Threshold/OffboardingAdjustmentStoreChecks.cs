namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingAdjustmentStoreChecks
{
    public static void MemoryOffboardingAdjustmentStoreFindsWhatItSaved()
    {
        var store = OffboardingAdjustmentStore.NewMemoryOffboardingAdjustmentStore();
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
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
        MemoryOffboardingAdjustmentStoreFindsWhatItSaved();
    }
}

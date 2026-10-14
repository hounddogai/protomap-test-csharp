namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class RegionalOffboardingSlotStoreChecks
{
    public static void MemoryRegionalOffboardingSlotStoreFindsWhatItSaved()
    {
        var store = RegionalOffboardingSlotStore.NewMemoryRegionalOffboardingSlotStore();
        var item = RegionalOffboardingSlot.NewRegionalOffboardingSlot("code-89", "name-527");
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
        MemoryRegionalOffboardingSlotStoreFindsWhatItSaved();
    }
}

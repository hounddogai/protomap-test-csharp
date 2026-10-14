namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingSlotStoreChecks
{
    public static void MemoryOffboardingSlotStoreFindsWhatItSaved()
    {
        var store = OffboardingSlotStore.NewMemoryOffboardingSlotStore();
        var item = OffboardingSlot.NewOffboardingSlot("code-730", "name-431");
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
        MemoryOffboardingSlotStoreFindsWhatItSaved();
    }
}

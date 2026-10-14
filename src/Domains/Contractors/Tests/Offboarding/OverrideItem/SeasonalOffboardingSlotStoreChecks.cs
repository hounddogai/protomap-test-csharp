namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class SeasonalOffboardingSlotStoreChecks
{
    public static void MemorySeasonalOffboardingSlotStoreFindsWhatItSaved()
    {
        var store = SeasonalOffboardingSlotStore.NewMemorySeasonalOffboardingSlotStore();
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("code-506", "name-254");
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
        MemorySeasonalOffboardingSlotStoreFindsWhatItSaved();
    }
}

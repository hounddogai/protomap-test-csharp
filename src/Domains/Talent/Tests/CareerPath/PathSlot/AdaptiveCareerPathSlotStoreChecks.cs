namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotStoreChecks
{
    public static void MemoryAdaptiveCareerPathSlotStoreFindsWhatItSaved()
    {
        var store = AdaptiveCareerPathSlotStore.NewMemoryAdaptiveCareerPathSlotStore();
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
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
        MemoryAdaptiveCareerPathSlotStoreFindsWhatItSaved();
    }
}

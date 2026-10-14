namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathSlotStoreChecks
{
    public static void MemoryCareerPathSlotStoreFindsWhatItSaved()
    {
        var store = CareerPathSlotStore.NewMemoryCareerPathSlotStore();
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
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
        MemoryCareerPathSlotStoreFindsWhatItSaved();
    }
}

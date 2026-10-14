namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayCycleStoreChecks
{
    public static void MemoryHolidayCycleStoreFindsWhatItSaved()
    {
        var store = HolidayCycleStore.NewMemoryHolidayCycleStore();
        var item = HolidayCycle.NewHolidayCycle("code-555", "title-397");
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
        MemoryHolidayCycleStoreFindsWhatItSaved();
    }
}

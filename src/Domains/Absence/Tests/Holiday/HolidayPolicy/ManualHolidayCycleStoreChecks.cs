namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class ManualHolidayCycleStoreChecks
{
    public static void MemoryManualHolidayCycleStoreFindsWhatItSaved()
    {
        var store = ManualHolidayCycleStore.NewMemoryManualHolidayCycleStore();
        var item = ManualHolidayCycle.NewManualHolidayCycle("code-972", "label-934");
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
        MemoryManualHolidayCycleStoreFindsWhatItSaved();
    }
}

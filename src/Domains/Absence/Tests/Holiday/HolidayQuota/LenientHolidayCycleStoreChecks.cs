namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class LenientHolidayCycleStoreChecks
{
    public static void MemoryLenientHolidayCycleStoreFindsWhatItSaved()
    {
        var store = LenientHolidayCycleStore.NewMemoryLenientHolidayCycleStore();
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
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
        MemoryLenientHolidayCycleStoreFindsWhatItSaved();
    }
}

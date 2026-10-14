namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayVarianceStoreChecks
{
    public static void MemoryHolidayVarianceStoreFindsWhatItSaved()
    {
        var store = HolidayVarianceStore.NewMemoryHolidayVarianceStore();
        var item = HolidayVariance.NewHolidayVariance("code-624", "name-300");
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
        MemoryHolidayVarianceStoreFindsWhatItSaved();
    }
}

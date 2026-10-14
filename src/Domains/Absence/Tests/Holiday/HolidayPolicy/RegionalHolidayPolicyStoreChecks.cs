namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class RegionalHolidayPolicyStoreChecks
{
    public static void MemoryRegionalHolidayPolicyStoreFindsWhatItSaved()
    {
        var store = RegionalHolidayPolicyStore.NewMemoryRegionalHolidayPolicyStore();
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
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
        MemoryRegionalHolidayPolicyStoreFindsWhatItSaved();
    }
}

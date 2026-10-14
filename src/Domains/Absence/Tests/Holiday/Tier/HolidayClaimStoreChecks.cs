namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayClaimStoreChecks
{
    public static void MemoryHolidayClaimStoreFindsWhatItSaved()
    {
        var store = HolidayClaimStore.NewMemoryHolidayClaimStore();
        var item = HolidayClaim.NewHolidayClaim("code-386", "name-637");
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
        MemoryHolidayClaimStoreFindsWhatItSaved();
    }
}

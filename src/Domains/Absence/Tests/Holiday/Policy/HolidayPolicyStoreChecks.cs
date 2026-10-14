namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayPolicyStoreChecks
{
    public static void MemoryHolidayPolicyStoreFindsWhatItSaved()
    {
        var store = HolidayPolicyStore.NewMemoryHolidayPolicyStore();
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
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
        MemoryHolidayPolicyStoreFindsWhatItSaved();
    }
}

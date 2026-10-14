namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayRuleStoreChecks
{
    public static void MemoryHolidayRuleStoreFindsWhatItSaved()
    {
        var store = HolidayRuleStore.NewMemoryHolidayRuleStore();
        var item = HolidayRule.NewHolidayRule("code-152", "title-551");
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
        MemoryHolidayRuleStoreFindsWhatItSaved();
    }
}

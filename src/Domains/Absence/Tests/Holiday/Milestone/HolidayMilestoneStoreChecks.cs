namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayMilestoneStoreChecks
{
    public static void MemoryHolidayMilestoneStoreFindsWhatItSaved()
    {
        var store = HolidayMilestoneStore.NewMemoryHolidayMilestoneStore();
        var item = HolidayMilestone.NewHolidayMilestone("code-918", "label-971");
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
        MemoryHolidayMilestoneStoreFindsWhatItSaved();
    }
}

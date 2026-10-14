namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class HolidayDraftStoreChecks
{
    public static void MemoryHolidayDraftStoreFindsWhatItSaved()
    {
        var store = HolidayDraftStore.NewMemoryHolidayDraftStore();
        var item = HolidayDraft.NewHolidayDraft("code-181", "title-41");
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
        MemoryHolidayDraftStoreFindsWhatItSaved();
    }
}

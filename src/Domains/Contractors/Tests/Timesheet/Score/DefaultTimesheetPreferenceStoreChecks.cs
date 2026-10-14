namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceStoreChecks
{
    public static void MemoryDefaultTimesheetPreferenceStoreFindsWhatItSaved()
    {
        var store = DefaultTimesheetPreferenceStore.NewMemoryDefaultTimesheetPreferenceStore();
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
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
        MemoryDefaultTimesheetPreferenceStoreFindsWhatItSaved();
    }
}

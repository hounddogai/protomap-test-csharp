namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreStoreChecks
{
    public static void MemoryLenientTimesheetScoreStoreFindsWhatItSaved()
    {
        var store = LenientTimesheetScoreStore.NewMemoryLenientTimesheetScoreStore();
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
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
        MemoryLenientTimesheetScoreStoreFindsWhatItSaved();
    }
}

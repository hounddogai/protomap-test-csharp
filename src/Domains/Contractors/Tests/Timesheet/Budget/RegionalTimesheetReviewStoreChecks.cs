namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RegionalTimesheetReviewStoreChecks
{
    public static void MemoryRegionalTimesheetReviewStoreFindsWhatItSaved()
    {
        var store = RegionalTimesheetReviewStore.NewMemoryRegionalTimesheetReviewStore();
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("code-692", "label-989");
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
        MemoryRegionalTimesheetReviewStoreFindsWhatItSaved();
    }
}

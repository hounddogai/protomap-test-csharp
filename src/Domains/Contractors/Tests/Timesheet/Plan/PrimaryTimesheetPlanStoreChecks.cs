namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryTimesheetPlanStoreChecks
{
    public static void MemoryPrimaryTimesheetPlanStoreFindsWhatItSaved()
    {
        var store = PrimaryTimesheetPlanStore.NewMemoryPrimaryTimesheetPlanStore();
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("code-861", "name-569");
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
        MemoryPrimaryTimesheetPlanStoreFindsWhatItSaved();
    }
}

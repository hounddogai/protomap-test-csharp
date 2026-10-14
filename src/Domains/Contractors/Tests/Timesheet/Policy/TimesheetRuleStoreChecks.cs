namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetRuleStoreChecks
{
    public static void MemoryTimesheetRuleStoreFindsWhatItSaved()
    {
        var store = TimesheetRuleStore.NewMemoryTimesheetRuleStore();
        var item = TimesheetRule.NewTimesheetRule("code-650", "title-661");
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
        MemoryTimesheetRuleStoreFindsWhatItSaved();
    }
}

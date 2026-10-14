namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultHolidayMilestoneStoreChecks
{
    public static void MemoryDefaultHolidayMilestoneStoreFindsWhatItSaved()
    {
        var store = DefaultHolidayMilestoneStore.NewMemoryDefaultHolidayMilestoneStore();
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("code-588", "label-452");
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
        MemoryDefaultHolidayMilestoneStoreFindsWhatItSaved();
    }
}

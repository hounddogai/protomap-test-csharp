namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientLeaveRequestForecastStoreChecks
{
    public static void MemoryLenientLeaveRequestForecastStoreFindsWhatItSaved()
    {
        var store = LenientLeaveRequestForecastStore.NewMemoryLenientLeaveRequestForecastStore();
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("code-718", "label-192");
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
        MemoryLenientLeaveRequestForecastStoreFindsWhatItSaved();
    }
}

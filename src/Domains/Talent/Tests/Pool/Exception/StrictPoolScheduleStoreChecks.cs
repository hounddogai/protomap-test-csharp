namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class StrictPoolScheduleStoreChecks
{
    public static void MemoryStrictPoolScheduleStoreFindsWhatItSaved()
    {
        var store = StrictPoolScheduleStore.NewMemoryStrictPoolScheduleStore();
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
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
        MemoryStrictPoolScheduleStoreFindsWhatItSaved();
    }
}

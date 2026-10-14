namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionStoreChecks
{
    public static void MemoryScheduledTownHallExceptionStoreFindsWhatItSaved()
    {
        var store = ScheduledTownHallExceptionStore.NewMemoryScheduledTownHallExceptionStore();
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
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
        MemoryScheduledTownHallExceptionStoreFindsWhatItSaved();
    }
}

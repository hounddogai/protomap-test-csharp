namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionStoreChecks
{
    public static void MemoryScheduledReorgExceptionStoreFindsWhatItSaved()
    {
        var store = ScheduledReorgExceptionStore.NewMemoryScheduledReorgExceptionStore();
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
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
        MemoryScheduledReorgExceptionStoreFindsWhatItSaved();
    }
}

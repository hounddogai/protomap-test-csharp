namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledBalanceDraftStoreChecks
{
    public static void MemoryScheduledBalanceDraftStoreFindsWhatItSaved()
    {
        var store = ScheduledBalanceDraftStore.NewMemoryScheduledBalanceDraftStore();
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("code-63", "name-491");
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
        MemoryScheduledBalanceDraftStoreFindsWhatItSaved();
    }
}

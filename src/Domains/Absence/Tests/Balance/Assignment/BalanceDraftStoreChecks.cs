namespace Acme.Workforce.Absence.Tests.Balance.Assignment;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BalanceDraftStoreChecks
{
    public static void MemoryBalanceDraftStoreFindsWhatItSaved()
    {
        var store = BalanceDraftStore.NewMemoryBalanceDraftStore();
        var item = BalanceDraft.NewBalanceDraft("code-298", "label-2");
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
        MemoryBalanceDraftStoreFindsWhatItSaved();
    }
}
